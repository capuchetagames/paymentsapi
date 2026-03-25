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
        
        try
        {
            var payment = new Payment
            {
                UserId = orderPlacedEvent.UserId,
                GameId = orderPlacedEvent.GameId,
                Price = orderPlacedEvent.Price,
                Status = "APPROVED"
            };
            
            _repository.Add(payment);
            Console.WriteLine($"Pagamento salvo no banco para UserId: {orderPlacedEvent.UserId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar pagamento no banco: {ex.GetType().Name} - {ex.InnerException?.Message}");
            throw;
        }
        
        try
        {
            await _rabbitMqService.PublishAsync(
                "payments.events",
                "payment.approved",
                new PaymentProcessedEvent(orderPlacedEvent.UserId, orderPlacedEvent.GameId, orderPlacedEvent.Email, orderPlacedEvent.Name, "APPROVED")
            );
            Console.WriteLine($"Evento payment.approved publicado para UserId: {orderPlacedEvent.UserId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao publicar evento no RabbitMQ: {ex.GetType().Name} - {ex.Message}");
            throw;
        }
    }
}