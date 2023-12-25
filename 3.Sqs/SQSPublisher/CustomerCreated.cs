﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQSPublisher
{
    internal class CustomerCreated
    {
        public required Guid Id { get; init; }
        public required string FullName { get; init; }
        public required string Email { get; init; }
        public required string GithubUserName { get; init; }
        public required DateTime DateOfBirth { get; init; }
    }
}
