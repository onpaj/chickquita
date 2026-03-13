namespace Chickquita.Application.Interfaces;

/// <summary>
/// Marker interface for requests that bypass authorization checks (e.g., webhook handlers).
/// </summary>
public interface IAnonymousRequest { }
