using Core.Entity;
using Core.Repository;

namespace Infrastructure.Repository;

public class PaymentRepository : EfRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }
}