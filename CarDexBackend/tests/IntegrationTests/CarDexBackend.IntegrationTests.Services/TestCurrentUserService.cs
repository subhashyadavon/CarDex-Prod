using CarDexBackend.Services;
using System;

namespace DefaultNamespace
{
    public class TestCurrentUserService : ICurrentUserService
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public bool IsAuthenticated => true;
    }
}
