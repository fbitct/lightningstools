using System;

namespace F4Utils.Campaign
{
    public class TeaFile
    {
        #region Public Fields

        public AirTaskingManager[] airTaskingManagers;
        public GroundTaskingManager[] groundTaskingManagers;
        public NavalTaskingManager[] navalTaskingManagers;
        public short numTeams;
        public Team[] teams;

        #endregion

        protected int _version;

        protected TeaFile()
        {
        }

        public TeaFile(byte[] bytes, int version)
            : this()
        {
            _version = version;
            var offset = 0;

            numTeams = BitConverter.ToInt16(bytes, offset);
            offset += 2;

            if (numTeams > 8)
                numTeams = 8;
            teams = new Team[numTeams];
            airTaskingManagers = new AirTaskingManager[numTeams];
            groundTaskingManagers = new GroundTaskingManager[numTeams];
            navalTaskingManagers = new NavalTaskingManager[numTeams];

            for (var i = 0; i < numTeams; i++)
            {
                var thisTeam = new Team(bytes, ref offset, version);
                teams[i] = thisTeam;
                airTaskingManagers[i] = new AirTaskingManager(bytes, ref offset, version);
                groundTaskingManagers[i] = new GroundTaskingManager(bytes, ref offset, version);
                navalTaskingManagers[i] = new NavalTaskingManager(bytes, ref offset, version);
            }
        }
    }
}