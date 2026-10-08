using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RepoRemoto.Notifications;

public class ConsoleNotificationService : INotificationService, IDisposable
{
    private readonly Subject<Notificacion> _subject = new();
    
    public IObservable<Notificacion> Observable => _subject.AsObservable();
    
    public void Notificar(TipoNotificacion tipo, string mensaje)
    {
        _subject.OnNext(new Notificacion(tipo, mensaje, DateTime.Now));
    }

    public void Dispose()
    {
        _subject.OnCompleted();
        _subject.Dispose();
    }
}