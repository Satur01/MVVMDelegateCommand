# MVVM VII ICommand y DelegateCommand
En esta serie de post hemos visto como el patrón [**MVVM**](https://saturninopimentel.com/tag/mvvm/) nos ayuda a eliminar el code-behind de nuestras vistas propiciando así la reutilización de código, en este post veremos que además de poder vincular propiedades a nuestra vista hecha en XAML también podemos agregar funcionalidad a través de comandos, es decir, en lugar de solo recibir o enviar notificaciones también podemos atar funcionalidad.

Para lograr esto necesitamos hacer una implementación de la interfaz  **ICommand**, esta interfaz tiene tres elementos (dos métodos y un evento), estos elementos deben ser implementados para después vincularlos con los objetos que heredan del control **ButtonBase** que contienen una propiedad **Command** que nos permite hacer uso de una expresión de [atado de datos](https://saturninopimentel.com/mvvm-ii-trabajando-con-atado-de-datos/) con la implementación de **ICommand**, veamos el siguiente ejemplo.

```language-csharp
 public class SendMessageCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return !string.IsNullOrEmpty((string)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter)
        {
            MessageDialog messageDialog = new MessageDialog(string.Format("Hola {0}!!", (string)parameter));
            await messageDialog.ShowAsync();
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this,new EventArgs());
            }
        }
    }
```
En este sencillo ejemplo enviaremos un mensaje siempre que el usuario proporcione su nombre haciendo uso de los métodos **CanExecute** y **Execute**, el primer método nos permite saber si el método **Execute** puede ser utilizado, como podrás notar he agregado un método extra llamado OnCanExecuteChanged el cual servirá para notificar a través del evento **CanExecuteChanged** que hay cambios y que debemos volver a evaluar si la funcionalidad puede o no ser ejecutada. Después de haber realizado esto podemos declarar una propiedad del tipo **ICommand** para poder hacer el atado de datos como se muestra a continuación.

```language-csharp
 private SendMessageCommand _sendMessageCommand;

 public ICommand SendMessageCommand
 {
    get { return _sendMessageCommand; }
 }
```
El atado de datos lo vamos a realizar como se muestra en el siguiente ejemplo, podrás darte cuenta que hacemos uso de la propiedad **CommandParameter** para enviar como parámetro el contenido de la propiedad Text del control **TxtMessage** mismo parámetro que reciben los métodos de la interfaz **ICommand**.
```
<StackPanel DataContext="{StaticResource VmMainPage}">
        <TextBox Name="TxtMessage" Text="{Binding Name, Mode=TwoWay}" />
        <Button HorizontalAlignment="Center"
                Command="{Binding SendMessageCommand}"
                CommandParameter="{Binding Text,
                                           ElementName=TxtMessage}"
                Content="Mensaje" />
    </StackPanel>
```

Con esto nosotros obtendremos un resultado como el que se muestra en las siguientes imágenes.

![MVVMCommandScreen1](/content/images/2015/06/wp_ss_20150623_0001.png)
![MVVMCommandScreen2](/content/images/2015/06/wp_ss_20150623_0003.png)
![MVVMCommandScreen3](/content/images/2015/06/wp_ss_20150623_0002.png)

####DelegateCommand
Es cierto que con el código anterior nos será suficiente para poder utilizar **ICommand** y eliminar el code-behind pero esto nos dejará con muchas clases con la implementación de **ICommand**, para evitar esto podemos hacer uso de una clase que se encargue de manejar todas las solicitudes de **ICommand** y así reutilizar esta implementación en todos los comandos necesarios. Para lograr esto solo tenemos que crear una clase llamada **DelegateCommand** (puedes hacer uso del nombre que gustes pero el más común es este) en la cual utilizaremos dos de los delegados definidos por el sistema [**Action**](https://msdn.microsoft.com/en-us/library/system.action(v=vs.110).aspx) y [**Func< T, TResult >**](https://msdn.microsoft.com/en-us/library/bb549151(v=vs.110).aspx) mismos que pasaremos como parámetros en el constructor como se muestra en el siguiente código.

```language-csharp
public class DelegateCommand : ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            return _canExecute();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute();
        }

       public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs());
        }
    }
```

El parámetro **_execute** nos servirá apuntar al método que debe ser ejecutado mientras que **_canExecute** nos servirá para evaluar si puede o no ser ejecutado el método, en caso de no proporcionar el parámetro **_canExecute** nuestro comando siempre podrá ser utilizado, de igual manera agregaremos el método  **OnCanExecuteChanged** que sirve para notificar que la evaluación proporcionada en el método **CanExecute** debe realizarse nuevamente, como podrás darte cuenta el trabajo de **ICommand** no se altera.
Con esto tu podrías utilizar la clase **DelegateCommand** y mantener la declaración de los métodos de evaluación y ejecución dentro de tu **ViewModel** facilitando aún más tu trabajo y con la capacidad de tener acceso a todos los elementos del **ViewModel** para no estar limitado únicamente al parámetro que utilizan los métodos declarados en **ICommand**, para la creación de una instancia puedes hacer uso de un código parecido al siguiente:

```language-csharp 
_sendMessageCommand=new DelegateCommand(SendMessageCommandExecute,SendMessageCommandCanExecute)
```

####Consejos para mejorar el rendimiento

Al hacer uso ya sea de las clases que implementen **ICommand** por separado o bien de **DelegateCommand** e instanciarlas reservaras cierta memoria para su funcionamiento y en entornos donde el uso de memoria sea limitado (teléfonos y tabletas) es necesario reducir al mínimo el uso de esta. Para lograr esto haremos uso de **Lazy<T>**, este genérico nos permite diferir la creación de la instancia hasta ser accedida por primera vez, logrando así evitar la reserva de memoria en funciones que pueden o no ser utilizadas. Para lograr esto necesitamos cambiar nuestro campo y hacer uso de **Lazy<T>** como se muestra a continuación.

```language-csharp
private Lazy<DelegateCommand> _sendMessageCommand; 
```
Además en el constructor de nuestro **ViewModel** haremos uso de una expresión lambda dentro del constructor del genérico **Lazy<T>** donde crearemos nuestra instancia de **DelegateCommand**
```language-csharp
  _sendMessageCommand =
                        new Lazy<DelegateCommand>(() => new DelegateCommand(SendMessageCommandExecute,SendMessageCommandCanExecute));
```
Por último modificaremos la propiedad **SendMessageCommand** para que regrese el valor dentro del campo _sendMessageCommand.
```language-csharp
public ICommand SendMessageCommand
 {
   get { return _sendMessageCommand.Value; }
 }
```
Con esto habremos mejorado el rendimiento de nuestra aplicación. Espero te resulte útil este post el código del ejemplo lo puedes descargar desde [aquí](https://github.com/Satur01/MVVMDelegateCommand), saludos!!!
