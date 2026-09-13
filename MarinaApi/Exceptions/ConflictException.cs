namespace MarinaApi.Exceptions;

/// <summary>
/// Se lanza cuando una operación viola una regla de negocio de unicidad o
/// estado (no un "no encontrado" ni un error de validación de formato).
/// El middleware global la convierte en una respuesta 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string mensaje) : base(mensaje) { }
}