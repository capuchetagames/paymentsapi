using Core.Dtos;
using Core.Models;

namespace PaymentsApi.Service;

public class OrderEventsConsumer : BackgroundService
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly IServiceScopeFactory _scopeFactory;

    public OrderEventsConsumer(IRabbitMqService rabbitMqService,IServiceScopeFactory scopeFactory)
    {
        _rabbitMqService = rabbitMqService;
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // exchange: "payments.events",
        // queue: "payments.process",
        // routingKey: "payment.requested",
        
        await _rabbitMqService.ConsumeAsync<OrderPlacedEvent>(
            exchange: "order.events",
            queue: "order.process",
            routingKey: "order.*",
            handler: Handle,
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    
    private async Task Handle(OrderPlacedEvent orderEvents)
    {
        Console.WriteLine($" Recebe uma Ordem de Compra  : {orderEvents.GameId} | {orderEvents.UserId} | {orderEvents.Price}");
        
        using var scope = _scopeFactory.CreateScope();

        var processor = scope.ServiceProvider
            .GetRequiredService<IPaymentProcessor>();

        await processor.ProcessAsync(orderEvents);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _rabbitMqService.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}