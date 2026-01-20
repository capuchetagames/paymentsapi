using Core.Dtos;

namespace Core.Models;

public interface IPaymentProcessor
{
    Task ProcessAsync(OrderPlacedEvent orderPlacedEvent);
}