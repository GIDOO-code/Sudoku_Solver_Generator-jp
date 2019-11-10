using System;
using System.Collections.Generic;
using System.Linq;

using GIDOO_space;

namespace GNPZ_sdk{
    partial class GNPZ_Analyzer{
        public bool GNP00_SueDeCoq( ){
            ALSLinkMan  fALS=new ALSLinkMan(this);  //(house�ɑ�����Z���Q�������N���X�Ƃ���ALS�𗘗p)
            if( fALS.ALS_Search(2)<=3 ) return false;   //+1��+2��fakeALS�𐶐�

            foreach( var ISPB in fALS.ALSLst.Where(p=> p.tfx>=18 && p.Size>=3) ){//�u���b�NfALS�I��
                if( ISPB.rcbRow.BitCount()<=1 || ISPB.rcbCol.BitCount()<=1 ) continue;�@//�u���b�N�e�͕����s�E��

                //���s(��)fALS�I��
                foreach( var ISPR in fALS.ALSLst.Where(p=> p.tfx<18 && p.Size>=3) ){�@//�sfALS�I��
                    if( (ISPR.rcbBlk&ISPB.rcbBlk)==0 ) continue; //�u���b�Nb0�ƌ�������
                    if( ISPR.rcbBlk.BitCount()<2 )     continue; //�s(��)fALS�͕����u���b�N

                    //�������̃Z���\���͓�����
                    if( (ISPB.B81&HouseCells[ISPR.tfx]) != (ISPR.B81&HouseCells[ISPB.tfx]) ) continue;

                    Bit81 IS = ISPB.B81&ISPR.B81;                //��������(Bit81�\��)
                    if( IS.Count<2 ) continue; �@                //��������2�Z���ȏ�
                    if( (ISPR.B81-IS).Count==0 ) continue;       //�s(��)ALS�Ɍ������ȊO�̕���������                    

                    Bit81 PB = ISPB.B81-IS;                      //(ISPB��IS�O)
                    Bit81 PR = ISPR.B81-IS;                      //(ISPR��IS�O)
                    int IS_FreeB = IS.AggregateFreeB(pBDL);      //(����������)
                    int PB_FreeB = PB.AggregateFreeB(pBDL);      //(ISPB��IS�O�̐���)
                    int PR_FreeB = PR.AggregateFreeB(pBDL);      //(ISPR��IS�O�̐���)
                    if( (IS_FreeB&PB_FreeB&PR_FreeB)>0 ) continue;

                    //A.DifSet(B)=A-B=A&(B^0x1FF)
                    int PB_FreeBn = PB_FreeB.DifSet(IS_FreeB);   //�u���b�N�̌������ɖ�������
                    int PR_FreeBn = PR_FreeB.DifSet(IS_FreeB);   //�s(��)�̌������ɖ�������

                    int sdqNC = PB_FreeBn.BitCount()+PR_FreeBn.BitCount();  //�������O�m��̐�����
                    if( (IS_FreeB.BitCount()-IS.Count) != (PB.Count+PR.Count-sdqNC) ) continue;

                    int elmB = PB_FreeB | IS_FreeB.DifSet(PR_FreeB); //�u���b�N�̏��O���� 
                    int elmR = PR_FreeB | IS_FreeB.DifSet(PB_FreeB); //�s(��)�̏��O����                
                    if( elmB==0 && elmR==0 ) continue;

                    bool ElmF=false;
                    foreach( var P in ISPB.GetRestCells(elmB) ){ P.CancelB|=P.FreeB&elmB; ElmF=true; }
                    foreach( var P in ISPR.GetRestCells(elmR) ){ P.CancelB|=P.FreeB&elmR; ElmF=true; }

                    if( !ElmF ) continue;
                   
                    //--- SueDeCoq fond ----------------------------------------------
                    SolCode=2;
                    SuDoQueEx_SolResult( ISPB, ISPR );
                    if( ISPB.Level>=3 || ISPB.Level>=3 ) Console.WriteLine("Level-3");
                    if( !SnapSaveGP(true) )  return true;
                    //foreach( var E in pBDL ) E.CancelB=0;
                }
            }
            return false;
        }

        private void SuDoQueEx_SolResult( UALS ISPB, UALS ISPR ){
            Result = ResultLong = "SueDeCoq";

            if( SolInfoDsp ){
                ISPB.UCellLst.ForEach(P=> P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr) );
                ISPR.UCellLst.ForEach(P=> P.SetNoBBgColor(P.FreeB,AttCr,SolBkCr) );

                string ptmp = "";
                ISPB.UCellLst.ForEach(p=>{ ptmp+=" r"+(p.r+1)+"c" + (p.c+1); } );

                string po = "\r Cells";
                if( ISPB.Level==1 ) po += "(block)  ";
                else{ po += "-"+ISPB.Level+"(block)"; }
                po += ": "+ISPB.ToStringRCN();

                po += "\r Cells" + ((ISPR.Level==1)? "": "-2");
                po += ((ISPR.tfx<9)? "(row)":"(col)");
                po += ((ISPR.Level==1)? "    ": "  ");
                po += ": "+ISPR.ToStringRCN();

                ResultLong += po;
            }
        }
    }
}