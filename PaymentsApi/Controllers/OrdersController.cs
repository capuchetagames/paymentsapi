using System.Security.Claims;
using Core;
using Core.Dtos;
using Core.Entity;
using Core.Models;
using Core.Repository;
using Microsoft.AspNetCore.Mvc;

namespace PaymentsApi.Controllers;

/// <summary>
/// Gerencia as operações CRUD para pedidos/pagamentos.
/// Utiliza autenticação distribuída via UserAPI.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IPaymentRepository paymentRepository, ICacheService cacheService, ILogger<OrdersController> logger)
    {
        _paymentRepository = paymentRepository;
        _cacheService = cacheService;
        _logger = logger;
    }
    
    /// <summary>
    /// Lista todos os pedidos. Requer permissão de Admin.
    /// </summary>
    /// <returns>Lista de todos os pedidos do sistema.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Payment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Get()
    {
        try
        {
            // Verificar se o usuário tem permissão de Admin
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (userRole != nameof(PermissionType.Admin))
            {
                _logger.LogWarning($"Usuário {username} tentou acessar lista completa de pedidos sem permissão de Admin.");
                return Forbid("Acesso negado. Apenas administradores podem visualizar todos os pedidos.");
            }

            _logger.LogInformation($"Admin {username} acessando lista completa de pedidos.");

            var ordersListKey = "ordersList";
            
            var cachedOrdersList = _cacheService.Get(ordersListKey);

            if (cachedOrdersList != null)
            {
                return Ok(cachedOrdersList);
            }
            
            var ordersList = _paymentRepository.GetAll();
            
            if(ordersList.Count > 0) 
                _cacheService.Set(ordersListKey, ordersList);
            
            return Ok(ordersList);
        }
        catch (Exception e)
        {
            _logger.LogError($"Erro ao buscar lista de pedidos: {e.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new 
            { 
                message = "Ocorreu um erro interno ao buscar os pedidos.",
                error = e.Message
            });
        }
    }

    /// <summary>
    /// Busca um pedido específico pelo seu ID.
    /// </summary>
    /// <param name="id">O ID do pedido a ser buscado.</param>
    /// <remarks>
    /// Usuários comuns só podem ver seus próprios pedidos.<br/>
    /// Administradores podem ver qualquer pedido.<br/>
    /// Os resultados são cacheados individualmente.
    /// </remarks>
    /// <returns>O objeto do pedido correspondente ao ID.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Payment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Get([FromRoute] int id)
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            _logger.LogInformation($"Usuário {username} buscando pedido ID: {id}");

            var orderKey = $"order-{id}";
            
            var cachedOrder = _cacheService.Get(orderKey);
            
            if (cachedOrder != null)
            {
                var cachedPayment = cachedOrder as Payment;
                
                // Verificar permissões para o objeto em cache
                if (userRole != nameof(PermissionType.Admin) && 
                    cachedPayment?.UserId.ToString() != userId)
                {
                    _logger.LogWarning($"Usuário {username} tentou acessar pedido {id} sem permissão.");
                    return Forbid("Você só pode visualizar seus próprios pedidos.");
                }
                
                return Ok(cachedOrder);
            }

            var order = _paymentRepository.GetById(id);
            
            if (order == null)
            {
                return NotFound(new { message = $"Pedido com ID {id} não encontrado." });
            }

            // Verificar se o usuário tem permissão para ver este pedido
            if (userRole != nameof(PermissionType.Admin) && 
                order.UserId.ToString() != userId)
            {
                _logger.LogWarning($"Usuário {username} tentou acessar pedido {id} de outro usuário.");
                return Forbid("Você só pode visualizar seus próprios pedidos.");
            }
            
            _cacheService.Set(orderKey, order);
            
            _logger.LogInformation($"Pedido {id} retornado com sucesso para usuário {username}");
            return Ok(order);
        }
        catch (Exception e)
        {
            _logger.LogError($"Erro ao buscar pedido {id}: {e.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new 
            { 
                message = "Erro interno do servidor.",
                error = e.Message
            });
        }
    }

    /// <summary>
    /// Obtém o histórico de pedidos do usuário autenticado.
    /// </summary>
    /// <returns>Lista de pedidos do usuário logado.</returns>
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(IEnumerable<Payment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetMyOrders()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("ID do usuário não encontrado no token.");
            }

            _logger.LogInformation($"Buscando pedidos do usuário {username} (ID: {userId})");

            var userOrdersKey = $"user-orders-{userId}";
            
            var cachedUserOrders = _cacheService.Get(userOrdersKey);
            
            if (cachedUserOrders != null)
            {
                return Ok(cachedUserOrders);
            }

            // Buscar pedidos do usuário específico
            var userOrders = _paymentRepository.GetAll()
                .Where(p => p.UserId == int.Parse(userId))
                .OrderByDescending(p => p.Id) // Ordenar por mais recente
                .ToList();

            if (userOrders.Any())
            {
                _cacheService.Set(userOrdersKey, userOrders);
            }

            _logger.LogInformation($"Retornados {userOrders.Count} pedidos para usuário {username}");
            return Ok(userOrders);
        }
        catch (Exception e)
        {
            _logger.LogError($"Erro ao buscar pedidos do usuário: {e.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new 
            { 
                message = "Erro interno do servidor.",
                error = e.Message
            });
        }
    }
    
    /// <summary>
    /// Deleta um pedido pelo ID. Apenas Admins podem deletar pedidos.
    /// </summary>
    /// <param name="id">O ID do pedido a ser deletado.</param>
    /// <returns>Nenhum conteúdo em caso de sucesso.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Delete([FromRoute] int id)
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            // Verificar se o usuário tem permissão de Admin
            if (userRole != nameof(PermissionType.Admin))
            {
                _logger.LogWarning($"Usuário {username} tentou deletar pedido {id} sem permissão de Admin.");
                return Forbid("Acesso negado. Apenas administradores podem deletar pedidos.");
            }

            _logger.LogInformation($"Admin {username} tentando deletar pedido ID: {id}");

            var order = _paymentRepository.GetById(id);
            if (order == null)
            {
                return NotFound(new { message = $"Pedido com ID {id} não encontrado." });
            }
            
            _paymentRepository.Delete(id);
            
            // Limpar cache relacionado
            _cacheService.Remove($"order-{id}");
            _cacheService.Remove("ordersList");
            _cacheService.Remove($"user-orders-{order.UserId}");
            
            _logger.LogInformation($"Pedido {id} deletado com sucesso pelo admin {username}");
            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError($"Erro ao deletar pedido {id}: {e.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new 
            { 
                message = "Ocorreu um erro interno.", 
                error = e.Message 
            });
        }
    }

    /// <summary>
    /// Endpoint público para verificar status do serviço de pedidos.
    /// </summary>
    /// <returns>Status do serviço.</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "healthy", 
            service = "PaymentsAPI - Orders", 
            timestamp = DateTime.UtcNow 
        });
    }
    
    
}