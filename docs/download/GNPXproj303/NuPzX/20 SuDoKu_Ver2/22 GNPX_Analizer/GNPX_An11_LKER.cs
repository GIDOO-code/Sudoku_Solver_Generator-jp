using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    public partial class NXGCellLinkGen: AnalyzerBaseV2{ 
        public bool  EmptyRectangle( ){
			Prepare();
			CeLKMan.PrepareCellLink(1);    //Generate strongLink
            
            for( int no=0; no<9; no++ ){        //Focused Number
                int noB = 1<<no;
                for( int bx=0; bx<9; bx++ ){    //Focused Block
                    int erB=pBDL.IEGetCellInHouse(bx+18,noB).Aggregate(0,(Q,P)=>Q|(1<<P.nx));
                    if(erB==0) continue;	

                    for( int er=0; er<9; er++ ){//Focused Cell in the Focused Block
                        int Lr=er/3, Lc=er%3;   //Block local Row and Column
                        int rxF = 7<<(Lr*3);    //7=1+2+4
                        int cxF = 73<<Lc;       //73=1+8+64
          
                        if( (erB&rxF)==0 || erB.DifSet(rxF)==0 )  continue; //Row Lr(Row Cndition Check)
                        if( (erB&cxF)==0 || erB.DifSet(cxF)==0 )  continue; //Column Lc(Column Cndition Check)          
                        if( erB.DifSet(rxF|cxF)>0 )               continue; //Row Lr and Column Lc(ER Cndition Check)                  
                        
                        int r1 = bx/3*3+Lr;     //Convert to Absolute Row
                        int c1 = (bx%3)*3+Lc;   //Convert to Absolute Column

                        foreach( var P in HouseCells[9+c1].IEGetUCeNoB(pBDL,noB).Where(Q=>Q.b!=bx) ){
                            foreach( var LK in CeLKMan.IEGetRcNoBTypB(P.rc,noB,1) ){
                                UCell Elm=pBDL[r1*9+LK.UCe2.c];
                                if( Elm.b!=bx && (Elm.FreeB&noB)>0 ){ //There is a Number that can be excluded
                                    EmptyRectangle_SolResult( no, bx, LK, Elm );
                                    if( !pAnMan.SnapSaveGP(true) )  return true;
                                }
                            }
                        }

                        foreach( var P in HouseCells[0+r1].IEGetUCeNoB(pBDL,noB).Where(Q=>Q.b!=bx) ){
                            foreach( var LK in CeLKMan.IEGetRcNoBTypB(P.rc,noB,1) ){
                                UCell Elm=pBDL[LK.UCe2.r*9+c1];
                                if( Elm.b!=bx && (Elm.FreeB&noB)>0 ){ //There is a Number that can be excluded
                                    EmptyRectangle_SolResult( no, bx, LK, Elm );
                                    if( !pAnMan.SnapSaveGP(true) )  return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        
        private void EmptyRectangle_SolResult(  int no, int bx, UCellLink PLK, UCell PElm ){
            int noB=(1<<no);
            SolCode = 2;
            Result = "EmptyRectangl #"+(no+1) +" in B"+(bx+1);
            PElm.CancelB = noB;    //Cancellation Number Setting
            if( !SolInfoB ) return;
              
            ResultLong = "EmptyRectangl";
            PLK.UCe1.SetNoBBgColor( noB, AttCr, SolBkCr2 );  //Mark Strong Links
            PLK.UCe2.SetNoBBgColor( noB, AttCr, SolBkCr2 );  //Mark Strong Links
               
            string st=""; 
            foreach( var Q in pBDL.IEGetCellInHouse(bx+18,noB) ){
                Q.SetNoBBgColor(noB,AttCr,SolBkCr);   //Empty Rectangle
                st += " "+Q.rc.ToRCString();
            }
            string msg="\r         digit: #"+(no+1) +"\r            ER: B"+(bx+1)+"("+st.ToString_SameHouseComp()+")";
            msg +="\r        S-Link: "+PLK.rc1.ToRCString()+"-"+PLK.rc2.ToRCString();
            msg +="\rEliminatedCell: "+PElm.rc.ToRCString();
            ResultLong = "EmptyRectangl"+msg;
        }
    }
}