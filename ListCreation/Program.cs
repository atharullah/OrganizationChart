using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace ListCreation
{
    
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, SPFieldType> columnType = new Dictionary<string, SPFieldType>();
            columnType.Add("s", SPFieldType.Text);
            columnType.Add("c", SPFieldType.Choice);
            columnType.Add("d", SPFieldType.DateTime);
            columnType.Add("p", SPFieldType.User);
            columnType.Add("mc", SPFieldType.MultiChoice);
            columnType.Add("mt",SPFieldType.)
            using (SPSite site = new SPSite(Metadata.siteURL))
            {
                SOMCommon common = new SOMCommon();
                common.setSPSite(site);

            }
        }

        public enum ListName
        {
            RCSA,
            RiskRegister,
            ITRisk
        }
        public enum RCSAColumn
        {
            IdentificationDate_d,
            ServiceName_c,
            ServiceProcessName_c,
            SOPName_s,
            Subprocess_s,
            RiskEventRef_s,
            InformationCriteria_c,
            RiskEvent,
            RiskEventDescription,
            ImpactType_c,
            ImpactAssessment_c,
            RiskLikelihood_c,
            RiskImpactRating_c,
            CombinedRiskAssessment_c,
            ControlName,
            ControlDescription,
            ControlDesignRatio_c,
            ControlOperatingEffectiveness_c,
            CombinedControlEffectivenessRating_c,
            ResidualRisk_c,
            ResponsiblePerson,
            Owner,
            Team_c,
            DueDate,
            ClosedDate,
            NewProposedDate_c,
            RequestorComments,
            OwnerComments,
            ApproverComments,
            ManagerApprover,
            Ageing,
            Status_c,
            AgeingBucket,
            SendTo_c
        }
    }

    public class Entity
    {
        string listname
        {
            get;
            set;
        }
        public string ListName { get; set; }
    }

    public class Metadata
    {
        public static string siteURL
        {
            get { return "http://fgit-sp-srv:3030/"; }
        }
    }
}
