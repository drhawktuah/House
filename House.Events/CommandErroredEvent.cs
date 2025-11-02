using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Exceptions;
using House.House.Attributes;
using House.House.Core;
using House.House.Services.Database;
using House.House.Services.Economy;

namespace House.House.Events;

public sealed class CommandErroredEvent : HouseCommandsNextEvent
{
    public CommandErroredEvent() : base("CommandErrored")
    {
    }

    public override async Task MainAsync(object sender, EventArgs eventArgs)
    {
        if (sender is not CommandsNextExtension _ || eventArgs is not CommandErrorEventArgs args)
        {
            return;
        }

        var context = args.Context;
        var exception = args.Exception;

        switch (exception)
        {
            case ChecksFailedException checks:
                await HandleFailedChecksAsync(context, checks);
                break;

            case CommandNotFoundException:
                await context.RespondAsync("`that command does not exist`");
                break;

            case ArgumentException or ArgumentNullException:
                await context.RespondAsync(exception.Message);
                break;

            /*
            case CoomerHTTPException httpEx:
                await context.RespondAsync($"`http {(int)httpEx.StatusCode} error`");
                break;
            */

            case UserNotFoundException notFoundException:
                await context.RespondAsync($"`{notFoundException.Message}`");
                break;

            case NoBalanceChangeProvidedException noBalanceChangeProvided:
                await context.RespondAsync($"`{noBalanceChangeProvided.Message}`");
                break;

            case UserAlreadyExistsException alreadyExistsException:
                await context.RespondAsync($"`{alreadyExistsException.Message}`");
                break;

            case EntityExistsException entityExistsException:
                await context.RespondAsync($"`{entityExistsException.Message}`");
                break;

            case EntityNotFoundException notFoundException:
                await context.RespondAsync($"`{notFoundException.Message} 1`");
                break;

            /*
            case CoomerCreatorNotFoundException creatorEx:
                await context.RespondAsync($"`creator '{creatorEx.Service}/{creatorEx.Username}' was not found`");
                break;

            case CoomerPostNotFoundException postEx:
                await context.RespondAsync($"`post with id '{postEx.PostId}' was not found`");
                break;

            case CoomerDeserializationException desEx:
                Console.WriteLine(desEx);

                await context.RespondAsync("`failed to parse data from coomer`");
                break;

            case CoomerClientException clientEx:
                Console.WriteLine(clientEx);

                await context.RespondAsync("`something went wrong in the coomer client`");
                break;

            case CoomerPostsNotFoundException postsEx:
                await context.RespondAsync($"`no posts found for {postsEx.Service}/{postsEx.Username}`");
                break;

            case CoomerServiceException serviceEx:
                Console.WriteLine(serviceEx);

                await context.RespondAsync("`a coomer service error occurred`");
                break;
            */

            case NotFoundException:
                break;

            default:
                Console.WriteLine($"[ERROR] {exception.Message}\n{exception.StackTrace}");

                break;
        }
    }

    private static async Task HandleFailedChecksAsync(CommandContext context, ChecksFailedException exception)
    {
        foreach (var check in exception.FailedChecks)
        {
            var message = check switch
            {
                IsOwnerAttribute => "`this command is restricted to only the owner, which you are not`",
                CooldownAttribute cooldown => $"`you can use this command again in {cooldown.GetRemainingCooldown(context).TotalSeconds:F1} seconds`",
                RequireUserPermissionsAttribute requireUserPermissions => $"`you are missing permissions {string.Join(", ", requireUserPermissions.Permissions)}`",
                RequireBotPermissionsAttribute requireBotPermissions => $"`i am missing permissions {string.Join(", ", requireBotPermissions.Permissions)}`",
                IsPlayerAttribute => "`you are not registered as a player in the database`",
                IsStaffAttribute => "`you are not a staff member with sufficient privileges to use this command`",
                IsStaffOrOwnerAttribute => "`you must be staff or a bot owner to use this command`",
                RequireDirectMessageAttribute => "`you must be in direct messages to use this command`",
                _ => $"`requirement check failed. requirement is: {check}`"
            };

            await context.RespondAsync(message);
            return;
        }
    }

}