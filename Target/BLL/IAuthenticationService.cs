namespace BLL
{
    public interface IAuthenticationService
    {
        Task<bool> AuthenticateAsync(string username, string password);
    }

    public class SimpleAuthenticationService : IAuthenticationService
    {
        public Task<bool> AuthenticateAsync(string username, string password)
        {
            // Простая проверка логина и пароля
            return Task.FromResult(username == "admin" && password == "admin");
        }
    }

    public class DatabaseAuthenticationService : IAuthenticationService
    {
        //private readonly DbContext _dbContext;

        //public DatabaseAuthenticationService(DbContext dbContext)
        //{
        //    _dbContext = dbContext;
        //}

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            return true;
            // Проверка логина и пароля через базу данных
          // var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
          //  return user != null;
        }
    }


 public class  SSOAuthenticationService : IAuthenticationService
    {
        //private readonly DbContext _dbContext;

        //public DatabaseAuthenticationService(DbContext dbContext)
        //{
        //    _dbContext = dbContext;
        //}

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            return true;
            // Проверка логина и пароля через базу данных
          // var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
          //  return user != null;
        }
    }

}
