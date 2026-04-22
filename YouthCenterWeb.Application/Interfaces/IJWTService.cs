using YouthCenterWeb.Models;

namespace YouthCenterWeb.InterFaces;

public interface IJwtService
{
    string GenerateToken(User user);
}