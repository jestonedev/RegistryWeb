namespace RegistryDb.Models.Entities
{
    public class JudgeBuildingAssoc
    {
        public int IdAssoc { get; set; }
        public int IdBuilding { get; set; }
        public int IdJudge { get; set; }
        public byte Deleted { get; set; }

        public virtual Building BuildingNavigation { get; set; }
        public virtual Judge JudgeNavigation { get; set; }
    }
}
