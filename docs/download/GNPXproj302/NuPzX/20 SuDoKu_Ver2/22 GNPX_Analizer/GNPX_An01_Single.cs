using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    //*==*==*==*==* Last Digit *==*==*==*==*==*==*==*==* 
    public class SimpleSingleGen: AnalyzerBaseV2{
        public SimpleSingleGen( GNPX_AnalyzerMan pAnMan ): base(pAnMan){ }

        public bool LastDigit( ){
            bool  SolFond=false;
            for( int tfx=0; tfx<27; tfx++ ){
                if( pBDL.IEGetCellInHouse(tfx,0x1FF).Count()==1 ){
                    SolFond=true;
                    var P=pBDL.IEGetCellInHouse(tfx,0x1FF).First();
                    P.FixedNo=P.FreeB.BitToNum()+1;                 
                    if( !chbConfirmMultipleCells )  goto LFond;
                }
            }

          LFond:
            if(SolFond){
                SolCode=1;
                Result="Last Digit";
                if( SolInfoB ) ResultLong="Last Digit";
                pAnMan.SnapSaveGP(); 
                return true;
            }
            return false;
        }

        //*==*==*==*==* Naked Single *==*==*==*==*==*==*==*==* 
        public bool NakedSingle( ){
            bool  SolFond=false;
            foreach( UCell P in pBDL.Where(p=>p.FreeBC==1) ){
                SolFond=true;
                P.FixedNo=P.FreeB.BitToNum()+1;      
                if(!chbConfirmMultipleCells)  goto LFond;
            }

          LFond:
            if(SolFond){
                SolCode=1;
                Result="Naked Single";
                if(SolInfoB) ResultLong="Naked Single";
                pAnMan.SnapSaveGP();
                return true;
            }
            return false;
        }

        //*==*==*==*==* Hidden Single *==*==*==*==*==*==*==*==*
        public bool HiddenSingle( ){
            bool  SolFond=false;
            for( int no=0; no<9; no++ ){
                int noB=1<<no;
                for( int tfx=0; tfx<27; tfx++ ){
                    if( pBDL.IEGetCellInHouse(tfx,noB).Count()==1 ){
                        SolFond=true;
                        var P=pBDL.IEGetCellInHouse(tfx,noB).First();
                        if(P.FreeBC==1)  continue;
                        P.FixedNo=no+1;             
                        if( !chbConfirmMultipleCells )  goto LFond;
                    }
                }
            }

          LFond:
            if(SolFond){
                SolCode=1;
                Result="Hidden Single";
                if( SolInfoB ) ResultLong="Hidden Single";
                pAnMan.SnapSaveGP();
                return true;
            }
            return false;
        }
    }
}