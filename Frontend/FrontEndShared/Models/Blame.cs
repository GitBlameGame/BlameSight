namespace FrontEndShared.Models
{

    public class Blame
    {
        public int id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string comment { get; set; }
        public string urgencyDescriptor { get; set; }
        public int lineNum { get; set; }
        public bool blameViewed { get; set; }
        public bool blameComplete { get; set; }

        public override string ToString()
        {
            string blameString = "";
            blameString += $"# ID: {id}\n";
            blameString += $"**Name**: {name}\n";
            blameString += $"**Path**: {path}\n";
            blameString += $"**Comment**: {comment}\n";
            blameString += $"**Urgency**: {urgencyDescriptor}\n";
            blameString += $"**Line Number**: {lineNum}\n";
            blameString += $"**Blame Viewed**: {blameViewed}\n";
            blameString += $"**Blame Complete**: {blameComplete}\n\n";

            return blameString;
        }
    }

}