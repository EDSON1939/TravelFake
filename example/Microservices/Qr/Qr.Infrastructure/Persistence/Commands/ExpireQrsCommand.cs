using Core.Infrastructure.Database.Commands;

namespace Qr.Infrastructure.Persistence.Commands;

public class ExpireQrsCommand : SqlCommandBase<long>
{
    public override string Name => "qr.UPDATE_QR_EXPIRADOS";
}