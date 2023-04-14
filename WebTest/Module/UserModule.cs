using SlickRepo;
using Models = WebTest.DB.Models;
using Dtos = WebTest.Dtos;

namespace WebTest.Module
{
    public class UserModule : SlickRepo<Models.User, Dtos.User>
    {
        public UserModule(WebTestContext ctx, SlickRepoConfig config) : base(ctx, config)
        {

        }
    }
}
