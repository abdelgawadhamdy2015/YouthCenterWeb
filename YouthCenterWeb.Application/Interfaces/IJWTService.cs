using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.InterFaces;

public interface IJwtService
{
    string GenerateToken(User user);
}