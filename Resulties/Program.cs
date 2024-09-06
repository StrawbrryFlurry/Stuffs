using Resulties;
using Resulties.Result;
using Resulties.TaskResult;

var user1 = await DoTheThing(1).Result();
Console.WriteLine($"User 1, Success: {user1.IsSuccess} Error: {user1.Error}");
var user2 = await DoTheThing(2).Result();
Console.WriteLine($"User 2, Success: {user2.IsSuccess} Error: {user2.Error}");
var user3 = await DoTheThing(3).Result();
Console.WriteLine($"User 3, Success: {user3.IsSuccess} Error: {user3.Error}");

static async TaskResult<AuthenticationResult> DoTheThing(int id) {
    var username = await GetUsernameForId(id);
    await AssertUsername(username);
    await Task.Delay(1000);
    await Task.Delay(1000);
    await Task.Delay(1000);
    return Authenticate(username);
}

static Result<AuthenticationResult> Authenticate(string username) {
    return new Error("User not found", "");
}

static Result<string> GetUsernameForId(int id) {
    if (id == 1) {
        return "Matt";
    }

    if (id == 2) {
        return "Steve";
    }

    return new Error("User not found", "");
}


static Result AssertUsername(string username) {
    if (username == "Matt") {
        return Result.Success;
    }

    return new Error("Username is not Matt", "");
}

namespace Resulties {
    public sealed class AuthenticationResult {
        public required Guid SessionId { get; init; }
    }
}