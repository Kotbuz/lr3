

namespace funny.Logic
{
    public interface IOldSexLogic
    {
          Task<string> GetJoke(bool sex, int old);
    }
}
