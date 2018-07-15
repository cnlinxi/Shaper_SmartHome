using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loT4WebApiSample.Model
{
    public class FaceSimilarInfo
    {
        public string faceId { get; set; }
        public string faceListId { get; set; }
        public int maxNumOfCandidatesReturned { get; set; }
    }
}
