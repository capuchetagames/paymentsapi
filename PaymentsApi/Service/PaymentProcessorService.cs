using Core.Dtos;
using Core.Entity;
using Core.Models;
using Core.Repository;

namespace PaymentsApi.Service;

public class PaymentProcessorService : IPaymentProcessor
{
    private readonly IPaymentRepository _repository;
    private readonly IRabbitMqService _rabbitMqService;
    
    public PaymentProcessorService(IPaymentRepository repository, IRabbitMqService rabbitMqService)
    {
        _rabbitMqService = rabbitMqService;
        _repository = repository;
    }
    
    public async Task ProcessAsync(OrderPlacedEvent orderPlacedEvent)
    {
        Console.WriteLine($" Processa o Pgto  : {orderPlacedEvent.UserId} | {orderPlacedEvent.GameId} | {orderPlacedEvent.Price}");
        
        var payment = new Payment
        {
            UserId = orderPlacedEvent.UserId,
            GameId =  orderPlacedEvent.GameId,
            Price = orderPlacedEvent.Price
        };
        
        _repository.Add(payment);

        // 2. Publica evento
        await _rabbitMqService.PublishAsync(
            "payments.events",
            "payment.approved",
            new PaymentProcessedEvent(orderPlacedEvent.UserId, orderPlacedEvent.GameId, orderPlacedEvent.Email, orderPlacedEvent.Name, "APPROVED")
        );
    }
}