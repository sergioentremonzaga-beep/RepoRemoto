namespace RepoRemoto.Notifications;

public enum TipoNotificacion
{
    Create,
    Update,
    Delete,
    Error 
}
public record Notificacion(TipoNotificacion Tipo, string Mensaje, DateTime Timestamp);
public interface INotificationService : IDisposable
{
    IObservable<Notificacion> Observable { get; }
    public void Notificar(TipoNotificacion tipo, string mensaje);
}
