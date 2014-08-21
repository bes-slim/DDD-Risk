﻿namespace Core.Facts.InvitationPhase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Core.InvitationPhase;

    using Xbehave;

    using Xunit;
    using Xunit.Should;

    public class InvitingAndAcceptingInvitations
    {
        [Scenario]
        [Example("Mat")]
        [Example("Bryan")]
        public void Host_can_invite_player_to_lobby(string playerName, Guid playerId, Guid invitationToken, Lobby lobby)
        {
            "Given a lobby"
                .Given(() => lobby = new Lobby(new CreateLobby("gameName", Guid.NewGuid(), "hostName")));

            "When we invite a player"
                .When(() =>
                    {
                        playerId = new Guid("8e17de3f-7833-47a6-8f92-cf220cf953e2");
                        invitationToken = new Guid("25e4349b-0f0a-479b-a7c7-78ebfb2ada00");
                        var command = new InvitePlayer(playerId, playerName, invitationToken);
                        lobby.Handle(command);
                    });

            "A player should be invited to the list of invited players"
                .Then(() =>
                    {
                        var @event = (PlayerInvited)lobby.Events.Last();
                        @event.PlayerId.ShouldBe(playerId);
                        @event.PlayerName.ShouldBe(playerName);
                        @event.InvitationToken.ShouldBe(invitationToken);
                    });
        }

        [Scenario]
        [Example("Mat")]
        [Example("Bryan")]
        public void Player_can_accept_invitation_to_lobby(string playerName, Guid invitationToken, Lobby lobby)
        {
            "Given a lobby"
                .Given(() => lobby = new Lobby(new CreateLobby("gameName", Guid.NewGuid(), "hostName")));

            "And an invited player"
                .And(() =>
                    {
                        var playerId = new Guid("8e17de3f-7833-47a6-8f92-cf220cf953e2");
                        invitationToken = new Guid("25e4349b-0f0a-479b-a7c7-78ebfb2ada00");
                        var command = new InvitePlayer(playerId, playerName, invitationToken);
                        lobby.Handle(command);
                    });

            "When the player accepts the invitation"
                .When(() =>
                    {
                        var command = new AcceptInvitation(invitationToken);
                        lobby.Handle(command);
                    });

            "A player should be added to the list of joined players"
                .Then(() =>
                    {
                        var @event = (InvitationAccepted)lobby.Events.Last();
                        @event.InvitationToken.ShouldBe(invitationToken);
                    });
        }

        [Scenario]
        [Example("Mat")]
        public void Player_cannot_accept_invitation_to_lobby_as_token_is_invalid(string playerName, Guid invitationToken, Lobby lobby)
        {
            "Given a lobby"
                .Given(() => lobby = new Lobby(new CreateLobby("gameName", Guid.NewGuid(), "hostName")));

            "And an invited player"
                .And(() =>
                {
                    var playerId = new Guid("8e17de3f-7833-47a6-8f92-cf220cf953e2");
                    invitationToken = new Guid("25e4349b-0f0a-479b-a7c7-78ebfb2ada00");
                    var command = new InvitePlayer(playerId, playerName, invitationToken);
                    lobby.Handle(command);
                });

            "When the player accepts the invitation and exception should throw"
                .When(() =>
                    {
                        var invalidInvitationToken = new Guid("7ad2e1e3-21f3-47df-9246-5a5faa1fdea4");
                        var command = new AcceptInvitation(invalidInvitationToken);
                        Assert.Throws<InvalidInvitationTokenException>(() => lobby.Handle(command));
                    });
        }

        [Scenario]
        public void Player_cannot_accept_invitation_as_lobby_is_full(Lobby lobby)
        {
            var invitationTokens = new List<Guid>();

            "Given a lobby"
                .Given(() => lobby = new Lobby(new CreateLobby("gameName", Guid.NewGuid(), "hostName")));

            "And and 6 invited player"
                .And(() =>
                {
                    for (var i = 0; i < 6; i++)
                    {
                        var playerId = Guid.NewGuid();
                        var invitationToken = Guid.NewGuid();
                        var playerName = "playerName";

                        invitationTokens.Add(invitationToken);
                        var command = new InvitePlayer(playerId, playerName, invitationToken);
                        lobby.Handle(command);
                    }
                });

            "And has 5 accepted invitations"
                .And(() =>
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var command = new AcceptInvitation(invitationTokens[i]);
                        lobby.Handle(command);
                    }
                });

            "When another player accepts the invitation a lobby full exception should throw"
                .When(() =>
                {
                    var command = new AcceptInvitation(invitationTokens[5]);
                    Assert.Throws<LobbyIsFullException>(() => lobby.Handle(command));
                });
        }

        [Scenario]
        public void Player_can_leave_the_lobby_after_accepting_invitation(Lobby lobby)
        {
            var invitationToken = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            "Given a lobby"
                .Given(() => lobby = new Lobby(new CreateLobby("gameName", Guid.NewGuid(), "hostName")));

            "And a player who has been invited"
                .And(() =>
                {
                    var playerName = "playerName";
                    var command = new InvitePlayer(playerId, playerName, invitationToken);
                    lobby.Handle(command);
                });

            "And who accepts"
                .And(() =>
                {
                    var command = new AcceptInvitation(invitationToken);
                    lobby.Handle(command);
                });

            "When they leave the lobby"
                .When(() =>
                {
                    var command = new LeaveLobby(playerId); 
                    lobby.Handle(command);
                });

            "Then a left lobby event should be raised"
                .Then(() =>
                {
                    var @event = (LeftLobby)lobby.Events.Last();
                    @event.PlayerId.ShouldBe(playerId);
                });
        }
    }
}