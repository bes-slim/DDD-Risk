﻿namespace Core.InvitationPhase
{
    using System;

    public class AcceptInvitation
    {
        public AcceptInvitation(Guid invitationToken)
        {
            this.InvitationToken = invitationToken;
        }

        public Guid InvitationToken { get; private set; }
    }
}