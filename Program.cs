using Discord;
using Discord.WebSocket;
using Npgsql.TypeMapping;
using Tippo.Web.Database;
using BCrypt.Net;
using TippoDiscord;
using TippoDiscord.Models;

class Program
{
    private DiscordSocketClient _client;
    private DatabaseManager dbManager = new DatabaseManager();

    public static Task Main(string[] args)
        => new Program().MainAsync();

    public async Task MainAsync()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMessages |
                GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(config);

        _client.Log += Log;
        _client.Ready += Ready;
        _client.MessageReceived += MessageReceived;
        _client.InteractionCreated += InteractionCreated;
        _client.SlashCommandExecuted += SlashCommandHandler;

        string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }

    private async Task Ready()
    {
        Console.WriteLine($"Bot online als {_client.CurrentUser}");

        ulong guildId = 1504282098784141314;

        var guild = _client.GetGuild(guildId);

        var removeRoleCommand = new SlashCommandBuilder()
        .WithName("removerole")
        .WithDescription("Entfernt einem User eine Rolle")
        .AddOption("user",
            ApplicationCommandOptionType.User,
            "Der User",
            isRequired: true)
        .AddOption("rolle",
            ApplicationCommandOptionType.Role,
            "Die Rolle",
            isRequired: true);

        var postMatchdayCommand = new SlashCommandBuilder()
        .WithName("postmatchday")
        .WithDescription("Postet einen Spieltag")
        .AddOption("wettbewerb",
            ApplicationCommandOptionType.String,
            "z.B. Bundesliga",
            isRequired: true)
        .AddOption("spieltag",
            ApplicationCommandOptionType.Integer,
            "Der Spieltag",
            isRequired: true);

        var meineTippsCommand = new SlashCommandBuilder()
        .WithName("meinetipps")
        .WithDescription("Zeigt dir deine abgegebenen Tipps");

        try
        {
            await guild.CreateApplicationCommandAsync(removeRoleCommand.Build());

            await guild.CreateApplicationCommandAsync(postMatchdayCommand.Build());

            await guild.CreateApplicationCommandAsync(meineTippsCommand.Build());

            Console.WriteLine("/removerole registriert");
            Console.WriteLine("/postmatchday registriert");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async Task MessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (message.Content == "!ping")
        {
            await message.Channel.SendMessageAsync("Pong!");
        }

        if (message.Content == "!registerpanel")
        {
            var button = new ComponentBuilder()
                .WithButton("Registrieren", "register_button", ButtonStyle.Success);

            var embed = new EmbedBuilder()
                .WithTitle("Tippo Registrierung")
                .WithDescription("Klicke auf den Button, um deinen Tippo-Account zu erstellen.")
                .WithColor(Color.Red)
                .Build();

            await message.Channel.SendMessageAsync(embed: embed, components: button.Build());
        }

        if (message.Content == "!loginpanel")
        {
            var button = new ComponentBuilder()
                .WithButton("Verknüpfen", "login_button", ButtonStyle.Primary);

            var embed = new EmbedBuilder()
                .WithTitle("Tippo-Account verknüpfen")
                .WithDescription("Klicke auf den Button, um dich mit deinem Tippo-Account zu verknüpfen.")
                .WithColor(Color.Red)
                .Build();

            await message.Channel.SendMessageAsync(embed: embed, components: button.Build());
        }
    }

    private async Task InteractionCreated(SocketInteraction interaction)
    {
        if (interaction is SocketMessageComponent component)
        {
            if (component.Data.CustomId == "register_button")
            {
                var modal = new ModalBuilder()
                    .WithTitle("Registrieren")
                    .WithCustomId("register_modal")
                    .AddTextInput("E-Mail", "register_email", TextInputStyle.Short, required: true)
                    .AddTextInput("Passwort", "register_password", TextInputStyle.Short, required: true)
                    .AddTextInput("Passwort wiederholen", "register_password_repeat", TextInputStyle.Short, required: true);

                await component.RespondWithModalAsync(modal.Build());
            }

            if (component.Data.CustomId == "login_button")
            {
                var modal = new ModalBuilder()
                    .WithTitle("Konto verknüpfen")
                    .WithCustomId("login_modal")
                    .AddTextInput("E-Mail", "login_email", TextInputStyle.Short, required: true)
                    .AddTextInput("Passwort", "login_password", TextInputStyle.Short, required: true);

                await component.RespondWithModalAsync(modal.Build());
            }

            if (component.Data.CustomId.StartsWith("tip_game_"))
            {
                string spielIdText = component.Data.CustomId.Replace("tip_game_", "");
                int spielId = int.Parse(spielIdText);

                var modal = new ModalBuilder()
                    .WithTitle("Tipp abgeben")
                    .WithCustomId($"tip_modal_{spielId}")
                    .AddTextInput("Tore Heimteam", "tipp_heim", TextInputStyle.Short, required: true)
                    .AddTextInput("Tore Gastteam", "tipp_gast", TextInputStyle.Short, required: true);

                await component.RespondWithModalAsync(modal.Build());
            }
        }

        if (interaction is SocketModal modalInteraction)
        {
            if (modalInteraction.Data.CustomId == "register_modal")
            {
                string username = modalInteraction.User.Username;

                string discord_user_id = modalInteraction.User.Id.ToString();

                string email = modalInteraction.Data.Components
                    .First(x => x.CustomId == "register_email").Value;

                string password = modalInteraction.Data.Components
                    .First(x => x.CustomId == "register_password").Value;

                string passwordRepeat = modalInteraction.Data.Components
                    .First(x => x.CustomId == "register_password_repeat").Value;

                if (password != passwordRepeat)
                {
                    await modalInteraction.RespondAsync("Die Passwörter stimmen nicht überein.", ephemeral: true);
                    return;
                }

                string password_hash = BCrypt.Net.BCrypt.HashPassword(modalInteraction.Data.Components.First(x => x.CustomId == "register_password").Value);

                bool success = dbManager.InsertUser(username, password_hash, email, discord_user_id);

                if (!success)
                {
                    await modalInteraction.RespondAsync(
                        "Diese E-Mail oder dieser Discord-Account ist bereits registriert.",
                        ephemeral: true
                    );
                    return;
                }

                ulong roleId = 1504562122342924348; // deine Rollen-ID

                var guildUser = modalInteraction.User as SocketGuildUser;

                if (guildUser == null)
                {
                    await modalInteraction.RespondAsync("Fehler: User konnte nicht gefunden werden.", ephemeral: true);
                    return;
                }

                var role = guildUser.Guild.GetRole(roleId);

                if (role == null)
                {
                    await modalInteraction.RespondAsync("Fehler: Rolle wurde nicht gefunden.", ephemeral: true);
                    return;
                }

                await guildUser.AddRoleAsync(role);

                await modalInteraction.RespondAsync(
                    $"Account `{email}` wurde registriert und du hast die Rolle erhalten.",
                    ephemeral: true
                );
            }

            if (modalInteraction.Data.CustomId == "login_modal")
            {
                string email = modalInteraction.Data.Components
                    .First(x => x.CustomId == "login_email").Value;

                string password = modalInteraction.Data.Components
                    .First(x => x.CustomId == "login_password").Value;

                UserCheck user = dbManager.SearchUser(email, password);

                if (user == null)
                {
                    await modalInteraction.RespondAsync(
                                    "Die angegebene E-Mail oder das Passwort sind fehlerhaft.",
                                    ephemeral: true
                                );
                }
                else
                {
                    if (user.ist_discord_verifiziert == 0)
                    {
                        bool hashCheck = BCrypt.Net.BCrypt.Verify(password, user.password_hash);

                        if (hashCheck)
                        {
                            string discordId = modalInteraction.User.Id.ToString();
                            bool linkdiscord = dbManager.LinkDiscord(email, discordId);

                            if (linkdiscord)
                            {
                                Console.WriteLine($"{email} mit Discord-ID {discordId} gelinkt");
                                var guildUser = modalInteraction.User as SocketGuildUser;

                                bool roleSuccess = await SetVerifiedRole(guildUser);

                                if (!roleSuccess)
                                {
                                    await modalInteraction.RespondAsync(
                                        "Account wurde verbunden, aber die Rolle konnte nicht vergeben werden.",
                                        ephemeral: true
                                    );
                                    return;
                                }
                                await modalInteraction.RespondAsync(
                                    "Account erfolgreich verbunden. Du hast jetzt die Rolle erhalten.",
                                    ephemeral: true
                                );
                            }
                            else
                            {
                                Console.WriteLine($"Fehler beim linken der Discord-ID {discordId}");
                            }
                        }
                    }
                    else if (user.ist_discord_verifiziert == 1)
                    {
                        await modalInteraction.RespondAsync(
                                        "Dein Account ist bereits verknüpft.",
                                        ephemeral: true
                                    );
                    }
                }
            }

            if (modalInteraction.Data.CustomId.StartsWith("tip_modal_"))
            {
                string spielIdText = modalInteraction.Data.CustomId.Replace("tip_modal_", "");
                int spielId = int.Parse(spielIdText);

                int tippHeim = int.Parse(
                    modalInteraction.Data.Components
                        .First(x => x.CustomId == "tipp_heim").Value
                );

                int tippGast = int.Parse(
                    modalInteraction.Data.Components
                        .First(x => x.CustomId == "tipp_gast").Value
                );

                string discordId = modalInteraction.User.Id.ToString();

                

                bool success = dbManager.SaveTipp(discordId, spielId, tippHeim, tippGast);

                if (success)
                {
                    await modalInteraction.RespondAsync(
                    $"Dein Tipp wurde gespeichert: {tippHeim}:{tippGast} für SpielID {spielId}",
                    ephemeral: true
                );
                }
                else
                {
                    await modalInteraction.RespondAsync(
                    $"Fehler beim Speichern des Tipps",
                    ephemeral: true
                );
                }
            }
        }
    }

    public async Task<bool> SetVerifiedRole(SocketGuildUser guildUser)
    {
        ulong roleId = 1504562122342924348;

        if (guildUser == null)
        {
            return false;
        }

        var role = guildUser.Guild.GetRole(roleId);

        if (role == null)
        {
            return false;
        }

        await guildUser.AddRoleAsync(role);

        return true;
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        if (command.Data.Name == "removerole")
        {
            var userOption = command.Data.Options
                .First(x => x.Name == "user");

            var roleOption = command.Data.Options
                .First(x => x.Name == "rolle");

            var guildUser = userOption.Value as SocketGuildUser;

            var role = roleOption.Value as SocketRole;

            if (guildUser == null || role == null)
            {
                await command.RespondAsync(
                    "User oder Rolle ungültig.",
                    ephemeral: true
                );
                return;
            }

            await guildUser.RemoveRoleAsync(role);

            await command.RespondAsync(
                $"Rolle `{role.Name}` wurde von {guildUser.Mention} entfernt.",
                ephemeral: true
            );
        }

        if (command.Data.Name == "postmatchday")
        {
            string wettbewerb = command.Data.Options
                .First(x => x.Name == "wettbewerb")
                .Value.ToString();

            int spieltag = Convert.ToInt32(
                command.Data.Options.First(x => x.Name == "spieltag").Value
            );

            var channel = command.Channel as SocketTextChannel;

            if (channel == null)
            {
                await command.RespondAsync("Channel konnte nicht gefunden werden.", ephemeral: true);
                return;
            }

            await command.RespondAsync(
                $"Spieltag {spieltag} für {wettbewerb} wird gepostet.",
                ephemeral: true
            );

            await PostMatchday(channel, wettbewerb, spieltag);
        }

        if (command.Data.Name == "meinetipps")
        {
            string discordId = command.User.Id.ToString();

            List<TippAnzeige> tipps = dbManager.GetMeineTipps(discordId);

            if (tipps.Count == 0)
            {
                await command.RespondAsync(
                    "Du hast bisher noch keine Tipps abgegeben.",
                    ephemeral: true
                );
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Deine Tipps")
                .WithColor(Color.Blue);

            foreach (var tipp in tipps)
            {
                embed.AddField(
                    $"{tipp.Heimteam} vs {tipp.Gastteam}",
                    $"Dein Tipp: **{tipp.TippHeim}:{tipp.TippGast}**\n" +
                    $"Anstoß: {tipp.Anstosszeit:dd.MM.yyyy HH:mm}",
                    false
                );
            }

            await command.RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }
    public async Task PostGame(SocketTextChannel channel, int spielId, string heim, string gast, DateTime anstosszeit)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"{heim} vs {gast}")
            .WithDescription($"Anstoß: {anstosszeit:dd.MM.yyyy HH:mm}")
            .WithColor(Color.Red)
            .Build();

        var button = new ComponentBuilder()
            .WithButton("Tipp abgeben", $"tip_game_{spielId}", ButtonStyle.Primary)
            .Build();

        await channel.SendMessageAsync(embed: embed, components: button);
    }

    public async Task PostMatchday(SocketTextChannel channel, string wettbewerb, int spieltag)
{
    List<Spiel> spiele = dbManager.GetMatchday(wettbewerb, spieltag);

    foreach (var spiel in spiele)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"{spiel.heimteam} vs {spiel.gastteam}")
            .WithDescription(
                $"**{wettbewerb} - Spieltag {spiel.spieltag}**\n" +
                $"Anstoß: {spiel.anstosszeit:dd.MM.yyyy HH:mm}"
            )
            .WithColor(Color.Red)
            .Build();

        var button = new ComponentBuilder()
            .WithButton(
                "Spiel tippen",
                $"tip_game_{spiel.spielId}",
                ButtonStyle.Primary
            )
            .Build();

        await channel.SendMessageAsync(embed: embed, components: button);
    }
}
}