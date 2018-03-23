#define STRICT
#include <windows.h>
#include <windowsx.h>
#include <stdio.h>
#include <string.h>
#include "fp.h"

#include <assert.h>
//#define TESTING

namespace D3530 {
	/// <summary>
	///
	/// </summary>
	public ref class D3530  {
	
public:
static int enddata=0;
static int ccc1=0;



/*void RS01(), RSlen(), RSseq(), RScmd(), RSdata();
void RS04(), RSstatus(), RS05(), RSbcc(), RS03();*/
void GetResponse(void);
void  FPtimer(HWND,UINT,UINT,DWORD);

void far *RetFun;

typedef void (PASCAL FAR *Ret) (struct RetData &);

static HANDLE RSportId;

RcvStates RcvState;
Answer  far ans;
char WholeRet[200];
struct RetData rtd;
LPSTR rdata=(LPSTR)WholeRet;
unsigned char Counter,CurCh,Seq=0,LastSeq=0;
unsigned long StatusFlag;

static bool WinDos=true;





DCB RSport;




//Windows95 5.10.98
COMMTIMEOUTS timeouts;
DWORD dwEvtMask;
//




UINT TimerId,rdCount,ResponseMsg=0x1099;
char Tmp[300],LastStr[300],Tmp1[300];
HWND RespWnd;

//Начало - 04.12.2000
//int  Ticks=0,LastCmd=0,Decimals=0,RetryCount=3;
int  Ticks=0,LastCmd=0,Decimals=2,RetryCount=3;  //последнее изменение 04.12.2000
//учитывает наличие копеек в гривнях
//Конец - 04.12.2000


int ShowAmount=1; //последнее изменение 17.02.2003

BOOL ReturnMode=TRUE;
int DebugCnt=0;

void (* RSfun[10])()= { *RS01,*RSlen,*RSseq,*RScmd,*RSdata,
											 *RS04,*RSstatus,*RS05,*RSbcc,*RS03 };

D3530()
{
RcvState=START;
}

/////////////////////////////////////////////////////////////////////////////
// comlog - log file for testing
void comlog(int cmd, char *infodata, LPARAM Status)
{


  FILE *stream1;
  stream1 = fopen("fpllog.dat", "at+");

      fputs(" \n",stream1);
      char  dat[200]="Код команды:";
      char  cmdx[20];
      itoa(cmd,cmdx,10);
      strcat(dat,cmdx);
      fputs(dat,stream1);

      strcpy(dat,"Данные: ");
      strcat(dat,infodata);
      fputs(" \n",stream1);
      fputs(dat,stream1);

      strcpy(dat,"Статус: ");
      _ltoa(cmd,cmdx,16);
      strcat(dat,cmdx);
      fputs(" \n",stream1);
      fputs(dat,stream1);


  fclose(stream1);
}



int RS232Receive(void)
	{
	char Buff[2];
   Buff[0]=0;
   DWORD Chars;
   static int SYNcnt=0;

   ReadFile(RSportId, (LPSTR *)Buff, 1, &Chars, NULL);

	if (Chars==1)
		 {
		 Ticks=0; CurCh=Buff[0];

       //beg               04.09.2013
       if(CurCh != 0x16)
          {
            rtd.RetDatX[ccc1++] = CurCh;
            rtd.RetDatX[ccc1] = 0;
          }
          else SYNcnt++;
       //end               04.09.2013

		 if (CurCh<0x20) switch (CurCh)
			 {
			 case PREAMBLE:
				 RcvState=START; RSfun[RcvState](); break;
			 case NACK:
				 RcvState=START; ans.RecError=4;
				 ans.ReadyFlag=1; break;
			 case 0x03:
				 if (RcvState!=CTRC) ans.RecError=8;
				 RcvState=CTRC; RSfun[RcvState](); break;
			 default:
				 //SYNcnt=0;               04.09.2013
				 RSfun[RcvState]();
			 }
       else
			 RSfun[RcvState]();

    		 return (1);
		 }
	else
		{
      DWORD ler=GetLastError();
		if (ler)
	  {		ClearCommError(RSportId,&ler,NULL);
			RcvState=START;
    }
	return (0);}
   }

//Windows98 5.10.98
HANDLE   GetRSId(void)
//int    GetRSId(void);
	{

  //Windows95 5.10.98
  return RSportId;
//  return RSport.Id;
  }

void RS01()                        // Preamble
	{
	if (CurCh==PREAMBLE) RcvState=LEN;
	ans.CalcSum=0;
	}

void RSlen()                       // Data length
	{
	if (CurCh>=0x2b)
		{ ans.Len=(unsigned char)(CurCh-0x2b); RcvState=SEQ; ans.CalcSum+=CurCh; }
	else
		RcvState=START;
	}

void RSseq()                       // Seq. block number
	{
	if (CurCh>=0x20)
		{ ans.Seq=(unsigned char)(CurCh-0x20); RcvState=CMD; ans.CalcSum+=CurCh; }
	else
		RcvState=START;
	}

void RScmd()                       // Command
	{
	ans.Cmd=CurCh; Counter=0; ans.CalcSum+=CurCh; *(ans.Data)='\0';
	if (ans.Len==0) RcvState=SEP; else RcvState=DATA;
	}

void RSdata()                      // Data bytes
	{
	if (Counter<ans.Len)
		{	*(ans.Data+Counter)=CurCh; Counter++; ans.CalcSum+=CurCh; }
	if (Counter==ans.Len)
		{ *(ans.Data+Counter)='\0'; RcvState=SEP; }
	}

void RS04()                        // Separator
	{
	if (CurCh==4)
		{ RcvState=STAT; Counter=0; ans.CalcSum+=CurCh; }
	else
		RcvState=START;
	}

void RSstatus()                    // Status bytes
	{
	if (Counter<6)
		{ *(ans.Status+Counter)=CurCh; Counter++; ans.CalcSum+=CurCh; }
	if (Counter==6)
		RcvState=EOT;
	}

void RS05()                        // Postamble
	{
	if (CurCh==POSTAMBLE)
		{ RcvState=CHKSUM; Counter=0; ans.CheckSum=0; ans.CalcSum+=CurCh; }
	else
		RcvState=START;
	}

void RSbcc()                       // CheckSum
	{
	if (Counter<4)
		{ ans.CheckSum=(ans.CheckSum<<4)+CurCh-0x30; Counter++; }
	if (Counter==4)
		RcvState=CTRC;
	}

void RS03()                       // Terminator
	{
	RcvState=START;
   enddata=1;
	if (CurCh==TERMINATOR) ans.ReadyFlag=1;
	}

#pragma argsused
int  LibMain(HINSTANCE hInstance,WORD wDataSeg,WORD wHeapSize,LPSTR lpszCmdLine)
	{
//Windows95 5.10.95
//	if (wHeapSize>0) UnlockData(0);
//
	return 1;
  }

LPSTR  CmdDescription(int Cmd)
	{
	if ((Cmd>=33)&&(Cmd<120)&&(CmdNames[Cmd-33][0]!='\0'))
		return CmdNames[Cmd-33];
	else
		return NotValidCmd;
  }

#pragma argsused
void   FPtimer(HWND hwnd, UINT msg, UINT Id, DWORD SysTime)
	{
	while (RS232Receive()==1);
	Ticks++;
	if ((Ticks>15)||(ans.ReadyFlag))
		{
		KillTimer(NULL,TimerId);
		GetResponse(); ans.ReadyFlag=1;
		if (((unsigned long)RetFun)<=0xffff)
			PostMessage(RespWnd,ResponseMsg,(WPARAM)LastCmd,(LPARAM)(&rtd));
		else
		  {
			((Ret)(RetFun)) (rtd);
      ReturnMode=TRUE;
			}
		}
  }


//Windows95 5.10.98
HANDLE   InitFPport(int PortNo,int Bps)
//

//int  _export InitFPport(int PortNo,int Bps)
	{
	char Cmd[32];

VirtualLock(RS232Receive,0x2000);

//Windows95 5.10.98
timeouts.ReadIntervalTimeout=0;
timeouts.ReadTotalTimeoutMultiplier=0;
//timeouts.ReadTotalTimeoutConstant=1000;
timeouts.ReadTotalTimeoutConstant=100;
timeouts.WriteTotalTimeoutMultiplier=0;
timeouts.WriteTotalTimeoutConstant=0;
//

//	int Cid=0,Err=0;
                                      
	ans.ReadyFlag=1;
//	sprintf(Cmd,"COM%d",PortNo);
	sprintf(Cmd,"\\\\.\\COM%d",PortNo);


   //Windows95 5.10.98

//   RSportId=CreateFile(Cmd, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, NULL);

   RSportId=CreateFile(Cmd, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, NULL, NULL);


   SetupComm(RSportId, 1024, 1024);
   //

//	Cid=OpenComm(Cmd,1024,1024);
    int err;
   //Windows95 5.10.98

        GetCommState(RSportId, &RSport);

RSport.BaudRate = Bps;
RSport.ByteSize = 8;
RSport.Parity = NOPARITY;
RSport.StopBits = ONESTOPBIT;



/*
	if (RSportId != INVALID_HANDLE_VALUE)
   //

//	if (Cid>=0)


		{
		sprintf(Cmd,"COM%d:%d,N,8,1",PortNo,Bps);
    	err=BuildCommDCB(Cmd,&RSport);
      if(err < 0 )return  INVALID_HANDLE_VALUE;
		}
*/
   //Windows95 5.10.98
//	else return (RSportId);
		SetCommState(RSportId,&RSport);
   err=SetCommTimeouts(RSportId,&timeouts);
 if (err <0)
	 return INVALID_HANDLE_VALUE;



//		SetCommMask(RSportId,EV_RXCHAR);

//	FlushFileBuffers(RSportId);
   PurgeComm(RSportId,PURGE_TXABORT | PURGE_RXABORT | PURGE_TXCLEAR | PURGE_RXCLEAR);


   return (RSportId);
   //

//	SetCommEventMask(Cid,EV_RXCHAR);
//	GetCommError(Cid,NULL);
//	FlushComm(Cid,0); FlushComm(Cid,1);

//	return 0;

	}

int   CloseFPport(void)
	{
      //Windows95 5.10.98
	FlushFileBuffers(RSportId);
   PurgeComm(RSportId,PURGE_TXABORT | PURGE_RXABORT | PURGE_TXCLEAR | PURGE_RXCLEAR);
   return CloseHandle(RSportId);
   	//

//	return CloseComm(RSport.Id);
  }

int  SendCmdAll(HWND hwnd,void ( *Fn),LPARAM UI,int Cmd,LPSTR Data)
	{
	if (hwnd==NULL) return SendCmdCbk(Fn,UI,Cmd,Data);
	else return SendCmdMsg(hwnd,UI,Cmd,Data);
	}

int  SendCmdMsg
(HWND hwnd,LPARAM UI,int Cmd, LPSTR Data)
	{
#ifndef TESTING
	if (!ans.ReadyFlag) return (-1);
#endif
	strcpy(LastStr,Data); LastCmd=Cmd; LastSeq=Seq;
	rtd.UserData=UI; rtd.CmdCode=Cmd;
	Seq++;
#ifdef TESTING
	char TestStr[80];
  StatusFlag=0L;
	ans.Status[0]=0xC0; ans.Status[1]=0xC0; ans.Status[2]=0xC0;
	ans.Status[3]=0xC0; ans.Status[4]=0xC0; ans.Status[5]=0xBA;
  ans.RecError=0; ans.ReadyFlag=1;
	switch (Cmd)
		{
		case 68:
			strcpy(ans.Data,"1200,1200");
			break;
		case 74:
			strncpy(ans.Data,ans.Status,6);
			break;
		case 90:
			strcpy(ans.Data,"1.10 02Feb98 1200,FFFF,00000000,6,DAT 12345678");
			break;
		case 83:
			strcpy(ans.Data,"0,2,ГРН ,2");
			break;
		case 97:
			strcpy(ans.Data,"22,00,00,00");
			break;
		case 99:
			strcpy(ans.Data,"DAT 12345678901234");
      break;
		case 62:
      strcpy(ans.Data,"01-01-98 11:11:11");
			break;
		case 110:
			strcpy(ans.Data,"1050,2000,0,0,8,2");
			break;
		case 67:
			strcpy(ans.Data,"0,100,2000,2,2");
			break;
		case 70:
			strcpy(ans.Data,"P,4050,3000,0");
			break;
		case 65:
			strcpy(ans.Data,"0,3050,0,0,0");
			break;
		default:
      ans.ReadyFlag=0;
			ans.Data[0]='\0';
		}
  RespWnd=hwnd; rdCount=0;
  ans.Len=strlen(ans.Data)+4;
	ans.CalcSum=0; ans.CheckSum=0;
	ans.Cmd=Cmd; ans.Seq=Seq;
	GetResponse(); ans.ReadyFlag=1;
	if (((unsigned long)RetFun)<=0xffff)
		PostMessage(RespWnd,ResponseMsg,(WPARAM)LastCmd,(LPARAM)(&rtd));
	else
	  {
		((Ret)(RetFun)) (rtd);
     ReturnMode=TRUE;
		}
	return 0;
#else
	StatusFlag=0L; rdCount=0;
	unsigned char Len,i,Ch;
	unsigned CheckSum=0;
	char MsgString[200],OemData[200];
	for (i=0; i<6; i++) ans.Status[i]=0;
	RespWnd=hwnd;

//  AnsiToOem(Data,OemData);
   unsigned int yloop;
   unsigned int len=strlen(Data);
   for(unsigned int iloop=0; iloop<=len;++iloop)
   {
   yloop=(unsigned int)(Data[iloop]) & 0xFF;
   if(!WinDos)OemData[iloop]=table[yloop];
    else OemData[iloop]=yloop;
   }
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   {
//   unsigned int yloop=(unsigned int)(Text[iloop]) & 0xFF;
//   Text[iloop]=table[yloop];
//   Text[iloop]=table[Text[iloop]];
//   }


//    strcpy(OemData,Data);

	ans.ReadyFlag=0; ans.RecError=0; ans.Data[0]='\0';
	Len=(unsigned char)(strlen(Data)+4);
	sprintf(MsgString,"\001%c%c%c%s\0050000\003",
										Len+0x20,(Seq%0x5F)+0x20,Cmd,OemData);
	for (i=0; i<Len; i++)
		{ Ch=*(MsgString+i+1); CheckSum+=Ch; }
	for (i=0; i<4; i++)
		{ MsgString[Len+4-i]=(char)(CheckSum%16+0x30); CheckSum/=16; }
	Len=(unsigned char)(strlen(MsgString));

   //Windows95 5.10.98
   FlushFileBuffers(RSportId);
   PurgeComm(RSportId,PURGE_TXCLEAR | PURGE_RXCLEAR);
   DebugCnt=0;
	DWORD Re;


   WriteFile(RSportId,MsgString,Len,&Re,NULL);
//   for(int ind=0; ind < strlen(MsgString); ++ind)
//     TransmitCommChar(RSportId,MsgString[ind]);

   /*
	FlushComm(RSport.Id,0); FlushComm(RSport.Id,1); DebugCnt=0;
	WriteComm(RSport.Id,MsgString,Len);
   */
	Ticks=0;

//   TimerId=SetTimer(NULL,0,130,FPtimer);

//	if (!TimerId) return (-2);

//   for(; Ticks<24;)
   for(; Ticks<50;)
   {while(enddata==0 && RS232Receive()==1){};
   if(ans.ReadyFlag==1)break;
   Ticks++;}

      enddata=0;
      ccc1=0;

		GetResponse(); ans.ReadyFlag=1;
		if (((unsigned long)RetFun)<=0xffff)
			PostMessage(RespWnd,ResponseMsg,(WPARAM)LastCmd,(LPARAM)(&rtd));
		else
		  {
			((Ret)(RetFun)) (rtd);
      ReturnMode=TRUE;
			}
//         }

//comlog(Cmd, Data, rtd.Status);

	return (0);
#endif
	}

#pragma argsused
int   SendCmdCbk(void ( *Fn),LPARAM UI,int Cmd,LPSTR Data)
	{
	RetFun=(void (PASCAL FAR *)(RetData far &))Fn;
	return SendCmdMsg(NULL,UI,Cmd,Data);
  }

int   SendLast(BOOL Increment)
	{
	if (!ans.ReadyFlag) return (-1);
	if (!Increment) Seq=LastSeq;
	return SendCmdMsg(RespWnd,rtd.UserData,LastCmd,LastStr);
  }

#pragma argsused
void GetResponse(void)
	{
	unsigned char StatByte;
	int i;
	if (ans.ReadyFlag)
    {
		if (ans.CheckSum!=ans.CalcSum) ans.RecError|=1;
		if (ans.Seq!=Seq) ans.RecError|=2;
		}
	for (i=0; i<6; i++) rtd.OrigStat[i]=(unsigned char)(ans.Status[i]&0x7f);
	if (!ans.ReadyFlag) StatusFlag|=TIMEOUT_ERROR;
	else if (ans.RecError&4) StatusFlag|=NACK_RECEIVED;
	else if (ans.RecError&1) StatusFlag|=PROTOCOL_ERROR; // !!????
	else
		{
		StatByte=ans.Status[0];
		if (StatByte)
			{
			if (StatByte&0x01) StatusFlag|=SYNTAX_ERROR;
			if (StatByte&0x02) StatusFlag|=INVALID_CMD;
			if (StatByte&0x04) StatusFlag|=INVALID_TIME;
			if (StatByte&0x10) StatusFlag|=PRINT_ERROR;
			if (StatByte&0x20) StatusFlag|=COMMON_ERROR;
			}
		StatByte=ans.Status[1];
		if (StatByte)
			{
			if (StatByte&0x01) StatusFlag|=SUM_OVERFLOW;
			if (StatByte&0x02) StatusFlag|=CMD_NOT_ALLOWED;
			if (StatByte&0x04) StatusFlag|=RAM_CLEARED;
			if (StatByte&0x08) StatusFlag|=PRINT_RESTART;
			if (StatByte&0x10) StatusFlag|=RAM_DESTROYED;
			if (StatByte&0x20) StatusFlag|=SERVICE_OPEN;
			}
		StatByte=ans.Status[2];
		if (StatByte)
			{
			if (StatByte&0x01) StatusFlag|=PAPER_OUT;
			if (StatByte&0x08) StatusFlag|=FISCAL_OPEN;
			if (StatByte&0x20) StatusFlag|=NONFISCAL_OPEN;
			}
		StatByte=ans.Status[4];
		if (StatByte)
			{
			if (StatByte&0x01) StatusFlag|=F_WRITE_ERROR;
			if (StatByte&0x04) StatusFlag|=F_ABSENT;
			//if (StatByte&0x08) StatusFlag|=F_LESS_30;
			if (StatByte&0x10) StatusFlag|=F_FULL;
			if (StatByte&0x20) StatusFlag|=F_COMMON_ERROR;
			}
		StatByte=ans.Status[5];
		if (StatByte)
			{
			if (StatByte&0x01) StatusFlag|=F_READ_ONLY;
			if (StatByte&0x02) StatusFlag|=F_FORMATTED;
			if (StatByte&0x04) StatusFlag|=F_CLOSE_ERROR;
			if (StatByte&0x08) StatusFlag|=F_FISCALIZED;
			if (StatByte&0x10) StatusFlag|=F_SER_NUM;
			if (StatByte&0x20) StatusFlag|=F_MODULE_NUM;
			}
		}

   unsigned int len=strlen(ans.Data);
   unsigned int yloop;
   for(unsigned int iloop=0; iloop<=len;++iloop)
   {
   yloop=(unsigned int)(ans.Data[iloop]) & 0xFF;
//   ans.Data[iloop]=table1[yloop];
//   if(!WinDos)ans.Data[iloop]=table1[yloop];
   if(!WinDos)ans.Data[iloop]=table1[yloop];
    else ans.Data[iloop]=yloop;
   }
   //	OemToAnsi(ans.Data,ans.Data);

	_fmemccpy((LPSTR)rdata,(LPSTR)ans.Data,'\0',128);
	if (strlen(ans.Data)>1)
		{
		switch (LastCmd)
			{
      case 50:; case 53:
				for (i=strlen(ans.Data); i>=1; i--) *(ans.Data+i+1)=*(ans.Data+i);
				*(ans.Data+1)=','; break;
			case 62:
				*(ans.Data+8)=','; break;
			case 90:
				for (i=0; i<20; i++)
					if (*(ans.Data+i)==' ') *(ans.Data+i)=',';
        break;
			}
		}
	rtd.Status=StatusFlag;
	rtd.CmdName=CmdDescription(LastCmd);
	rtd.SendStr=LastStr;
	rtd.Whole=rdata;
	if (ans.Data[0]=='\0')
		rdCount=0;
	else
		{
    rtd.RetItem[0]=(LPSTR)ans.Data;
		for (i=0; ans.Data[i]; i++) if (ans.Data[i]==',')
			{
      rdCount++; ans.Data[i]='\0';
			rtd.RetItem[rdCount]=(LPSTR)(ans.Data+i+1);
			}
		rdCount++;
		}
	rtd.Count=rdCount;
	}

void   SetMessageNum(UINT Message)
	{
	ResponseMsg=Message;
	}

int   SetDecimals(int DecCount)
	{
  if ((DecCount>3)||(DecCount<0)) return (-1);
	Decimals=DecCount;
	return (0);
  }

int   SetShowAmount(int Show)
	{
  if ((Show!=1)&&(Show!=0)) return (-1);
	ShowAmount=Show;
	return (0);
  }

int   OpenNonfiscalReceipt(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,38,"");
	}

int   CloseNonfiscalReceipt(HWND hwnd,void ( *Fn),LPARAM UI)
	{
   strcpy(Tmp,"");
//	strcpy(Tmp,"00");
//	if (Number) Tmp[0]='1';
//	if (Time)   Tmp[1]='1';
	return SendCmdAll(hwnd,Fn,UI,39,Tmp);
	}

int   PrintNonfiscalText(HWND hwnd,void ( *Fn),LPARAM UI,LPSTR Text)
	{
//   unsigned int len=strlen(Text);
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   Text[iloop]=table[Text[iloop]];
	return SendCmdAll(hwnd,Fn,UI,42,Text);
	}

int   SetHeaderFooter(HWND hwnd,void ( *Fn),LPARAM UI,int Line,LPSTR Text)
	{
  if(Line >= 0 && Line <= 7)Tmp[0]=(char)(Line+0x30);
    else {
  if(Line == 'C' || Line == 'L' || Line == 'J' || Line == 'I')Tmp[0]=(unsigned char)(Line);
    else return -1;}
//   unsigned int len=strlen(Text);
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   Text[iloop]=table[Text[iloop]];
	strcpy((Tmp+1),Text);
	return SendCmdAll(hwnd,Fn,UI,43,Tmp);
	}


int   AdvancePaper(HWND hwnd,void ( *Fn),LPARAM UI,int Lines)
	{
	sprintf(Tmp,"%d",Lines);
	return SendCmdAll(hwnd,Fn,UI,44,Tmp);
	}

int   DisplayTextLL(HWND hwnd,void ( *Fn),LPARAM UI,LPSTR uText)
	{


        char dat[140]={0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                      0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                      0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                      0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

        char Text[40]={0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                      0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

        int AAA, AAA1, fff;

        char prGE[]={/*prg*/0x10,0x5B,0x26,0x20, 0x10, 0x4B,0xD0,0x84,0x21,0x10, 0x40, 0x00};

        bool enableGE=false;

        char PutGEEn[] = {/*enable*/0x10, 0x5B,0x25,0x10, 0x41,0x00};
        char PutGe[] = {/*put*/0x20,0x00};
        char PutGEDi[] = {/*disable*/0x10, 0x5B,0x25,0x10,0x40,0x00};

        char PrE[] = {/*prg*/0x10, 0x5B, 0x26,0x21, 0x74,0x61,0xC8,0x45,0xC0, 0x00};
        char PutE[] = { 0x21, 0x00};

        char posL[] = {0x10, 0x5b, 0x48, 0x10, 0x54, 0x10, 0x58, 0x00};
        if(strlen(uText) > 20)uText[20]=0;

        if(WinDos)
        {
	return SendCmdAll(hwnd,Fn,UI,35,uText);
        }

        else
        {
        WinDos=true;

//set letters
        if(strchr(uText,0xA5) || strchr(uText,0xB4))AAA=SendCmdAll(hwnd,Fn,UI,100,prGE);
        if(strchr(uText,0xAA) || strchr(uText,0xBA))AAA1=SendCmdAll(hwnd,Fn,UI,100,PrE);

//Set Pos:
        fff=SendCmdAll(hwnd,Fn,UI,100,posL);

        int j=0;
        for(int i=0; i<strlen(uText); ++i)
         {

         if ((unsigned char)uText[i]>=0x80)Text[i]=DisplayTable[uText[i]&0x7F];
            else Text[i]=uText[i];

           switch((unsigned char)uText[i])
            {
              case 0xA5:
              case 0xB4:
                 {
                   if(!enableGE)
                       {
                        enableGE=true;
                        strcat(dat,PutGEEn);
                       }
                   strcat(dat,PutGe);
                 }break;
              case 0xAA:
              case 0xBA:
                 {
                   if(!enableGE)
                       {
                        enableGE=true;
                        strcat(dat,PutGEEn);
                       }
                   strcat(dat,PutE);
                 }break;
              case 0x20:
              case 0x21:
              if(enableGE)
                {
                        enableGE=false;
                        strcat(dat,PutGEDi);
                }
              default:  dat[strlen(dat)]=Text[i];
            }
         }

          if(enableGE)
              {
                strcat(dat,PutGEDi);
                enableGE=false;
              }
          if(strlen(dat))SendCmdAll(hwnd,Fn,UI,100,dat);
          WinDos=false;
          return 0;
         }

	}

int   OpenFiscalReceipt(HWND hwnd,void ( *Fn),LPARAM UI,
	 DWORD Operator=1,	LPSTR Text="0000", DWORD TillNumber=1, BOOL TaxCheck=true)
	{

	sprintf(Tmp,"%05lu,%s,%05lu",Operator, Text, TillNumber);
//   if(TaxCheck)strcat(Tmp,",I");

   strcat(Tmp,",I");

//	sprintf(Tmp,"00001,0000,00000,I");

	return SendCmdAll(hwnd,Fn,UI,48,Tmp);
	}

int   OpenRepaymentReceipt(HWND hwnd,void ( *Fn),LPARAM UI,
	 DWORD Operator=1,	LPSTR Text="0000", DWORD TillNumber=1, BOOL TaxCheck=true)
	{

	sprintf(Tmp,"%05lu,%s,%05lu",Operator, Text, TillNumber);
//   if(TaxCheck)strcat(Tmp,",I");
   strcat(Tmp,",I");
//	sprintf(Tmp,"00001,0000,00000,I");

	return SendCmdAll(hwnd,Fn,UI,85,Tmp);
	}



int   GetTaxRates(HWND hwnd,void ( *Fn),LPARAM UI,LPSTR psw, LPSTR Start,LPSTR End)
	{
	sprintf(Tmp,"%s,%s,%s",psw,Start,End);
	return SendCmdAll(hwnd,Fn,UI,50,Tmp);
  }

int  SaleArticle(HWND hwnd,void ( *Fn),LPARAM UI,
					 bool sign, int numart, double qwant, double perc, double  dc)
{
if(perc != 0. && dc != 0.)return -1;
if(qwant < 0.)return -1;
if(perc < -99.99 && perc > 99.99)return -1;

char FprmStr[200];

if(sign)FprmStr[0]=0;
  else strcpy(FprmStr,"-");

char art1[30];
itoa(numart,art1,10);
strcat(FprmStr,art1);

if(qwant > 0)
	{
	 strcat(FprmStr,"*");
    char qw[30];
    sprintf(qw,"%-#.3lf",qwant);
    strcat(FprmStr,qw);
   }

if(perc != 0)
	{
	 strcat(FprmStr,",");
    char prc[30];
    sprintf(prc,"%-#.2lf",perc);
    strcat(FprmStr,prc);
   }

if(dc != 0)
	{
	 strcat(FprmStr,";");
    char dcstr[30];
    sprintf(dcstr,"%-#.2lf",dc);
    strcat(FprmStr,dcstr);
   }

	return SendCmdAll(hwnd,Fn,UI,58,FprmStr);

}

int  SaleArticleAndDisplay(HWND hwnd,void ( *Fn),LPARAM UI,
					 bool sign, int numart, double qwant, double perc, double  dc)
{
if(perc != 0. && dc != 0.)return -1;
if(qwant < 0.)return -1;
if(perc < -99.99 && perc > 99.99)return -1;

char FprmStr[200];

if(sign)FprmStr[0]=0;
  else strcpy(FprmStr,"-");

char art1[30];
itoa(numart,art1,10);
strcat(FprmStr,art1);

if(qwant > 0)
	{
	 strcat(FprmStr,"*");
    char qw[30];
    sprintf(qw,"%-#.3lf",qwant);
    strcat(FprmStr,qw);
   }

if(perc != 0)
	{
	 strcat(FprmStr,",");
    char prc[30];
    sprintf(prc,"%-#.2lf",perc);
    strcat(FprmStr,prc);
   }

if(dc != 0)
	{
	 strcat(FprmStr,";");
    char dcstr[30];
    sprintf(dcstr,"%-#.2lf",dc);
    strcat(FprmStr,dcstr);
   }

	return SendCmdAll(hwnd,Fn,UI,52,FprmStr);

}

int   GetLastTaxRates(HWND hwnd,void ( *Fn),LPARAM UI, LPSTR psw)
	 {

//	 sprintf(Tmp,"%s,%s",Start,End);
	 return SendCmdAll(hwnd,Fn,UI,50,psw);
	 }



int   SetTaxNumber(HWND hwnd,void ( *Fn),LPARAM UI, LPSTR tn, int type)
	 {

    if(type == 1)
    strcpy(Tmp,"%s,1");
    else{
    if(type == 0)
    strcpy(Tmp,"%s,0");
    else return -1;}
	 return SendCmdAll(hwnd,Fn,UI,98,Tmp);
	 }


int   SubTotal(HWND hwnd,void ( *Fn),LPARAM UI,BOOL Print,BOOL Display, double Percent=0.0, double DS=0.0)
	{

			char FormatStr[200];
   		char TmpTT[200];

//<Print><Display>[,<[Sign]Percent>]
	strcpy(Tmp,"00");
	if (Print)   Tmp[0]='1';
	if (Display) Tmp[1]='1';

if(Percent != 0.0 && DS != 0.0)return -1;

if(Percent != 0.0 && DS == 0.0)
        {
			strcpy(FormatStr,"%s,%-#.2lf");
   		sprintf(TmpTT,FormatStr,Tmp,Percent);
        }

if(Percent == 0.0 && DS != 0.0)
        {
			strcpy(FormatStr,"%s;%-#.2lf");
   		sprintf(TmpTT,FormatStr,Tmp,DS);
        }

if(Percent == 0.0 && DS == 0.0)
        {
			strcpy(FormatStr,"%s");
		   sprintf(TmpTT,FormatStr,Tmp);
        }


//   SendCmdAll(hwnd,Fn,UI,51,"00,");
	return SendCmdAll(hwnd,Fn,UI,51,TmpTT);


/*
   if(Percent != 0.0)
   {
   sprintf(TmpTT,FormatStr,Tmp,Percent);
	return SendCmdAll(hwnd,Fn,UI,51,TmpTT);
   }
   else
	return SendCmdAll(hwnd,Fn,UI,51,Tmp);
*/

	}




int   Total(HWND hwnd,void ( *Fn),LPARAM UI,
		LPSTR Comment,char PaidCode,double Amount)
	{
//[<Line1>][<Lf><Line2>]<Tab>[<PaidMode>]<[Sign]Amount>
//   unsigned int len=strlen(Comment);
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   Comment[iloop]=table[Comment[iloop]];
	char TmpFloat[200];
	strcpy(TmpFloat,"%s\tP%-#.2lf");
  if (PaidCode=='\0') TmpFloat[3]='P';
	else TmpFloat[3]=PaidCode;
//	TmpFloat[strlen(TmpFloat)-3]=Decimals+0x30;
  sprintf(Tmp,TmpFloat,Comment,Amount);


//   SendCmdAll(hwnd,Fn,UI,53,"\tP0.0");


	return SendCmdAll(hwnd,Fn,UI,53,Tmp);
	}



int   PrintFiscalText(HWND hwnd,void ( *Fn),LPARAM UI,LPSTR Text)
	{
//   unsigned int len=strlen(Text);
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   Text[iloop]=table[Text[iloop]];
	return SendCmdAll(hwnd,Fn,UI,54,Text);
	}


int   CloseFiscalReceipt(HWND hwnd,void ( *Fn),LPARAM UI)
	{
   //SendCmdAll(hwnd,Fn,UI,33,"");
	return SendCmdAll(hwnd,Fn,UI,56,"");
	}

int    ResetReceipt(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,57,"");
	}

int   SetDateTime(HWND hwnd,void ( *Fn),LPARAM UI,LPSTR Date,LPSTR Time)
	{
	sprintf(Tmp,"%s %s",Date,Time);
	return SendCmdAll(hwnd,Fn,UI,61,Tmp);
	}

int   GetDateTime(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,62,"");
	}

int   LastFiscalClosure(HWND hwnd,void ( *Fn),LPARAM UI,int option)
	{
   if(option == 1)
	return SendCmdAll(hwnd,Fn,UI,64,"1");
   else
    {
     if(option == 0)return SendCmdAll(hwnd,Fn,UI,64,"0");
     else return -1;
    }
	}

        

int   GetCurrentTaxes(HWND hwnd,void ( *Fn),LPARAM UI, int  option)
	{
    switch (option)
     {
      case 0: return SendCmdAll(hwnd,Fn,UI,65,"0");
      case 1: return SendCmdAll(hwnd,Fn,UI,65,"1");
      case 2: return SendCmdAll(hwnd,Fn,UI,65,"2");
      case 3: return SendCmdAll(hwnd,Fn,UI,65,"3");
      default: return -1;
     }

	}

int   FiscalMemoryLookup(HWND hwnd,void ( *Fn),LPARAM UI,int Closure)
	{
	sprintf(Tmp,"%04d",Closure);
	return SendCmdAll(hwnd,Fn,UI,66,Tmp);
	}

int   GetCurrentSums(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,67,"");
	}

int   GetFreeClosures(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,68,"");
	}

int   FiscalClosure(HWND hwnd,void ( *Fn),LPARAM UI, LPSTR psw, char Option)
	{
	sprintf(Tmp,"%s,%c",psw,Option);
	return SendCmdAll(hwnd,Fn,UI,69,Tmp);
	}

int    ArticulsReport(HWND hwnd,void ( *Fn),LPARAM UI, LPSTR psw, char param)
	{
        if(param != 'S' && param != 'P' && param != 'G')return -1;
	sprintf(Tmp,"%s,%c",psw,param);
	return SendCmdAll(hwnd,Fn,UI,111,Tmp);
	}

int   PrintDiagnosticInfo(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,71,"");
	}

int   PrinterBeep(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,80,"");
	}

//int   Fiscalise(HWND hwnd,void ( *Fn),LPARAM UI,LPSTR SerialNum,
//                               LPSTR RegNum)
int   Fiscalise(HWND hwnd,void ( *Fn),LPARAM UI, LPSTR psw, LPSTR SerialNum,
                               LPSTR RegNum, int regtype)
	{
	strcpy(Tmp,psw);
   strcat(Tmp,",");
	strcat(Tmp,SerialNum);
   if(RegNum == "")return SendCmdAll(hwnd,Fn,UI,72,Tmp);
   strcat(Tmp,",");
   strcat(Tmp,RegNum);
   strcat(Tmp,",");
   if(regtype == 1)strcat(Tmp,"1");
    else
    {
     if(regtype == 0)strcat(Tmp,"0");
      else return -1;
    }
	return SendCmdAll(hwnd,Fn,UI,72,Tmp);
	}


int   PrintFiscalMemoryByNum(HWND hwnd,void ( *Fn),LPARAM UI, LPSTR psw, int Start,
																						int End)
//int   PrintFiscalMemoryByNum(HWND hwnd,void ( *Fn),LPARAM UI,int Start,
//																						int End,BOOL Short)
	{
	sprintf(Tmp,"%s,%04d,%04d",psw,Start,End);
//	if (!Short) Tmp[strlen(Tmp)-1]='\0';
	return SendCmdAll(hwnd,Fn,UI,73,Tmp);
	}

int   GetStatus(HWND hwnd,void ( *Fn),LPARAM UI,BOOL NoWait)
	{
	if (NoWait) return SendCmdAll(hwnd,Fn,UI,74,"X");
	else return SendCmdAll(hwnd,Fn,UI,74,"W");
	}


int   GetFiscalClosureStatus(HWND hwnd,void ( *Fn),LPARAM UI,BOOL Current)
	{
	if (Current) return SendCmdAll(hwnd,Fn,UI,76,"T");
	else return SendCmdAll(hwnd,Fn,UI,76,"");
	}

int   PrintReportByDate(HWND hwnd,void ( *Fn),LPARAM UI, LPSTR psw,
																			 LPSTR Start,LPSTR End)
	{
	sprintf(Tmp,"%s,%s,%s",psw,Start,End);
	return SendCmdAll(hwnd,Fn,UI,79,Tmp);
	}

int   SetMulDecCurRF(HWND hwnd,void ( *Fn),LPARAM UI,
		LPSTR psw, int Dec,LPSTR enabled, double taxA, double taxB, double taxC, double taxD)
	{
//   unsigned int len=strlen(Currency);
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   Currency[iloop]=table[Currency[iloop]];
	sprintf(Tmp,"%s,%d,%s,%#4.2f,%#4.2f,%#4.2f,%#4.2f",psw,Dec,enabled,taxA,taxB,taxC,taxD);
	return SendCmdAll(hwnd,Fn,UI,83,Tmp);
	}

/*
int   SetMulDecCurRF(HWND hwnd,void ( *Fn),LPARAM UI,
		int Mult,int Dec,LPSTR Currency,int RatesFewer)
	{
//   unsigned int len=strlen(Currency);
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   Currency[iloop]=table[Currency[iloop]];
	sprintf(Tmp,"%d,%d,%s ,%d",Mult,Dec,Currency,RatesFewer);
	return SendCmdAll(hwnd,Fn,UI,83,Tmp);
	}
*/

int   GetMulDecCurRF(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,83,"");
	}


int   SetTaxType(HWND hwnd,void ( *Fn),LPARAM UI, int type)
	{
//   unsigned int len=strlen(Text);
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   Text[iloop]=table[Text[iloop]];
   if(type == 1)return SendCmdAll(hwnd,Fn,UI,84,"1");
   if(type == 0)return SendCmdAll(hwnd,Fn,UI,84,"0");
   return -1;
	}

/*
int   SetFMNumber(HWND hwnd,void ( *Fn), LPARAM UI,LPSTR FMNumber)
	{
	return SendCmdAll(hwnd,Fn,UI,87,FMNumber);
	}
*/

int   ProgramTestArea(HWND hwnd,void ( *Fn),LPARAM UI,BOOL Test)
	{
	if (Test) return SendCmdAll(hwnd,Fn,UI,89,"T");
	else return SendCmdAll(hwnd,Fn,UI,89," ");
  }

int   GetDiagnosticInfo(HWND hwnd,void ( *Fn),LPARAM UI,BOOL Calc)
	{
	if (Calc) return SendCmdAll(hwnd,Fn,UI,90,"1");
	else return SendCmdAll(hwnd,Fn,UI,90,"0");
	}

int   SetCountrySerial(HWND hwnd,void ( *Fn),LPARAM UI,int Country,LPSTR Serial)
	{
   //страна - Украина - 2
   //номер это 10 байт - две буквы и 8 цифр
	sprintf(Tmp,"%d,%s",Country,Serial);
	return SendCmdAll(hwnd,Fn,UI,91,Tmp);
	}

int   SetFiscalNumber(HWND hwnd,void ( *Fn),LPARAM UI, LPSTR Fiscal)
	{
   //номер это 10 байт - одни цифры
	sprintf(Tmp,"%s",Fiscal);
	return SendCmdAll(hwnd,Fn,UI,92,Tmp);
	}

int   PrintFiscalMemoryByDate(HWND hwnd,void ( *Fn),LPARAM UI,
		LPSTR psw, LPSTR Start,LPSTR End)
	{
	sprintf(Tmp,"%s,%6s,%6s",psw,Start,End);
//	if (!Short) Tmp[strlen(Tmp)-1]='\0';
	return SendCmdAll(hwnd,Fn,UI,94,Tmp);
	}

int   PrintReportByNum(HWND hwnd,void ( *Fn),LPARAM UI,
																		 LPSTR psw,	int Start,int End)
	{
	sprintf(Tmp,"%s,%04d,%04d",psw,Start,End);
	return SendCmdAll(hwnd,Fn,UI,95,Tmp);
	}



int   GetCurrentTaxRates(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,97,"");
	}

int   GetTaxNumber(HWND hwnd,void ( *Fn),LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,99,"");
	}

int   ClearDisplay(HWND hwnd,void ( *Fn),LPARAM UI)
	{
  return SendCmdAll(hwnd,Fn,UI,33,"");
	}

int   DisplayDateTime(HWND hwnd,void ( *Fn),LPARAM UI)
	{
  return SendCmdAll(hwnd,Fn,UI,63,"");
	}

int   DisplayTextUL(HWND hwnd,void ( *Fn),LPARAM UI,LPSTR uText)
	{

        char dat[140]={0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                      0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                      0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                      0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

        char Text[40]={0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
                      0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

        int AAA, AAA1, fff;

        char prGE[]={/*prg*/0x10,0x5B,0x26,0x20, 0x10, 0x4B,0xD0,0x84,0x21,0x10, 0x40, 0x00};

        bool enableGE=false;

        char PutGEEn[] = {/*enable*/0x10, 0x5B,0x25,0x10, 0x41,0x00};
        char PutGe[] = {/*put*/0x20,0x00};
        char PutGEDi[] = {/*disable*/0x10, 0x5B,0x25,0x10,0x40,0x00};

        char PrE[] = {/*prg*/0x10, 0x5B, 0x26,0x21, 0x74,0x61,0xC8,0x45,0xC0, 0x00};
        char PutE[] = { 0x21, 0x00};

        char posU[] = {0x10, 0x5b, 0x48, 0x10, 0x40, 0x10, 0x58, 0x00};
        if(strlen(uText) > 20)uText[20]=0;

        if(WinDos)
        {
	return SendCmdAll(hwnd,Fn,UI,47,uText);
        }

        else
        {
        WinDos=true;

//set letters
        if(strchr(uText,0xA5) || strchr(uText,0xB4))AAA=SendCmdAll(hwnd,Fn,UI,100,prGE);
        if(strchr(uText,0xAA) || strchr(uText,0xBA))AAA1=SendCmdAll(hwnd,Fn,UI,100,PrE);

//Set Pos:
        fff=SendCmdAll(hwnd,Fn,UI,100,posU);

        int j=0;
        for(int i=0; i<strlen(uText); ++i)
         {

         if ((unsigned char)uText[i]>=0x80)Text[i]=DisplayTable[uText[i]&0x7F];
            else Text[i]=uText[i];

           switch((unsigned char)uText[i])
            {
              case 0xA5:
              case 0xB4:
                 {
                   if(!enableGE)
                       {
                        enableGE=true;
                        strcat(dat,PutGEEn);
                       }
                   strcat(dat,PutGe);
                 }break;
              case 0xAA:
              case 0xBA:
                 {
                   if(!enableGE)
                       {
                        enableGE=true;
                        strcat(dat,PutGEEn);
                       }
                   strcat(dat,PutE);
                 }break;
              case 0x20:
              case 0x21:
              if(enableGE)
                {
                        enableGE=false;
                        strcat(dat,PutGEDi);
                }
              default:  dat[strlen(dat)]=Text[i];
            }
         }

          if(enableGE)
              {
                strcat(dat,PutGEDi);
                enableGE=false;
              }
          if(strlen(dat))SendCmdAll(hwnd,Fn,UI,100,dat);
          WinDos=false;
          return 0;
         }

	}

int   DisplayFreeText(HWND hwnd,void ( *Fn),LPARAM UI,LPSTR Str)
	{
//   unsigned int len=strlen(Str);
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   Str[iloop]=table[Str[iloop]];
	return SendCmdAll(hwnd,Fn,UI,100,Str);
	}



int   ServiceInputOutput(HWND hwnd,void ( *Fn),LPARAM UI, double Sum)
	{
   char FormatStr[200];
	strcpy(FormatStr,"%-#.2lf");
   FormatStr[4]=(char)(Decimals+0x30);
	sprintf(Tmp,FormatStr,Sum);
	if(Sum != 0.)return SendCmdAll(hwnd,Fn,UI,70,Tmp);
       else return SendCmdAll(hwnd,Fn,UI,70,"");
	}

int   DayInfo(HWND hwnd, void ( *Fn), LPARAM UI)
	{
	return SendCmdAll(hwnd,Fn,UI,110,"");
  }

int    OpenDrawer(HWND hwnd, void ( *Fn), LPARAM UI, int msec)
{

char tmper[100];


  if(msec <= 4)return SendCmdAll(hwnd,Fn,UI,106,"");
       else
          {
				sprintf(tmper,"%d",msec);
            return SendCmdAll(hwnd,Fn,UI,106,tmper);
          }
}

int    GetReceiptInfo(HWND hwnd, void ( *Fn), LPARAM UI)
{
  return SendCmdAll(hwnd,Fn,UI,103,"");
}

int    MakeReceiptCopy(HWND hwnd, void ( *Fn), LPARAM UI, char Count)
{
   if(Count == 2)strcpy(Tmp,"2");
     else strcpy(Tmp,"1");
	//sprintf(Tmp,"%c",Count);
   return SendCmdAll(hwnd,Fn,UI,109,Tmp);
}

int    CutReceipt(HWND hwnd, void ( far *Fn), LPARAM UI)
{
	return SendCmdAll(hwnd,Fn,UI,45,"");
}

int    OperatorsReport(HWND hwnd, void ( far *Fn), LPARAM UI, LPSTR psw)
{
	return SendCmdAll(hwnd,Fn,UI,105,psw);
}

int    GetLastReceipt(HWND hwnd, void ( far *Fn), LPARAM UI)
{
	return SendCmdAll(hwnd,Fn,UI,113,"");
}

int    GetOperatorInfo(HWND hwnd, void ( far *Fn), LPARAM UI, int Operator)
{
	sprintf(Tmp,"%d",Operator);
   return SendCmdAll(hwnd,Fn,UI,112,Tmp);
}

int    SetOperatorPassword(HWND hwnd, void ( *Fn), LPARAM UI, int NumOper, LPSTR OldPass, LPSTR NewPass)
{
			char FormatStr[200];
			strcpy(FormatStr,"%d,%s,%s");
         sprintf(Tmp,FormatStr,NumOper,OldPass,NewPass);
   		return SendCmdAll(hwnd,Fn,UI,101,Tmp);
}

int    SetOperatorName(HWND hwnd, void ( *Fn), LPARAM UI, int NumOper, LPSTR Password, LPSTR Name)
{
//   unsigned int len=strlen(Name);
//   for(unsigned int iloop=0; iloop<len;++iloop)
//   Name[iloop]=table[Name[iloop]];
			char FormatStr[200];
			strcpy(FormatStr,"%d,%s,%s");
         sprintf(Tmp,FormatStr,NumOper,Password,Name);
   		return SendCmdAll(hwnd,Fn,UI,102,Tmp);
}

int    GetCommonArticleInfo(HWND hwnd, void ( *Fn), LPARAM UI)
{
   		return SendCmdAll(hwnd,Fn,UI,107,"I");
}

int    ProgrammingArticle(HWND hwnd, void ( *Fn), LPARAM UI, char tax, int numart,
     int group, double price, LPSTR psw, LPSTR name)
{
//if(numart <= 0)return -1;
//if(tax != 'А' && tax != 'Б' && tax != 'В' && tax != 'Г' && tax != 'Д' &&
//     (unsigned char)tax != (unsigned char)0x80 && (unsigned char)tax != (unsigned char)0x81 &&
//     (unsigned char)tax != (unsigned char)0x82 && (unsigned char)tax != (unsigned char)0x83 &&
//     (unsigned char)tax != (unsigned char)0x84)return -2;
if(group < 1 || group > 99)return -3;  //проверить, что больше нуля
//if(price <= 0.)return -4;

  char tmper[400];
  sprintf(tmper,"P%c%d,%d,%-#.2lf,%s,%s",tax,numart,group,price,psw,name);
  return SendCmdAll(hwnd,Fn,UI,107,tmper);
}

int    DeleteArticle(HWND hwnd, void ( *Fn), LPARAM UI, int numart, LPSTR psw)
{
if(numart < 0)return -1;

char frmt[200];
char tmper[30];

if(numart == 0)
 {
  strcpy(frmt,"DA,");
  strcat(frmt,psw);
  return SendCmdAll(hwnd,Fn,UI,107,frmt);
 }
else
 {
  strcpy(frmt,"D");
  sprintf(tmper,"%d,",numart);
  strcat(frmt,tmper);
  strcat(frmt,psw);
  return SendCmdAll(hwnd,Fn,UI,107,frmt);
 }
}

int    ChangeArticlePrice(HWND hwnd, void ( *Fn), LPARAM UI, int numart, double price, LPSTR psw)
{
if(numart <= 0)return -1;

  char tmper[200];
  sprintf(tmper,"C%d,%-#.2lf,%s",numart,price,psw);
  return SendCmdAll(hwnd,Fn,UI,107,tmper);
}

int    GetFirstFreeArticleNum(HWND hwnd, void ( *Fn), LPARAM UI)
{
   		return SendCmdAll(hwnd,Fn,UI,107,"X");
}

int    GetArticleInfo(HWND hwnd, void ( *Fn), LPARAM UI, int numart)
{
  if(numart < 0)return -1;

  char frmt[200];
  char tmper[30];
  strcpy(frmt,"R");
  sprintf(tmper,"%d",numart);
  strcat(frmt,tmper);
  return SendCmdAll(hwnd,Fn,UI,107,frmt);
}


int    GetFirstArticleNum(HWND hwnd, void ( *Fn), LPARAM UI)
{
   		return SendCmdAll(hwnd,Fn,UI,107,"F");
}

int    GetNextArticleNum(HWND hwnd, void ( *Fn), LPARAM UI)
{
   		return SendCmdAll(hwnd,Fn,UI,107,"N");
}

int  SetWinDosPage(bool win)
{
 WinDos=win;
}

bool  GetWinDosPage()
{
 return WinDos;
}

int    LogoLoad(HWND hwnd, void ( *Fn), LPARAM UI, LPSTR psw, int row, char *Data)
{
  if(row < 0 || row > 95)return -1;

  char tmper[400];
  sprintf(tmper,"%s,%d,%s",psw,row,Data);
  return SendCmdAll(hwnd,Fn,UI,115,tmper);
}

int    SetAdminPassword(HWND hwnd, void ( *Fn), LPARAM UI, LPSTR OldPsw, LPSTR NewPsw)
{
  char tmper[40];
  sprintf(tmper,"%s,%s",OldPsw,NewPsw);
  return SendCmdAll(hwnd,Fn,UI,118,tmper);
}

int    ClearOperatorPassword(HWND hwnd, void ( *Fn), LPARAM UI, int oper, LPSTR psw)
{
  if(oper < 1 || oper > 15)return -1;
  char tmper[40];
  sprintf(tmper,"%d,%s",oper,psw);
  return SendCmdAll(hwnd,Fn,UI,119,tmper);
}

int    OperatorDataNull(HWND hwnd, void ( *Fn), LPARAM UI, int oper, LPSTR psw)
{
  if(oper < 1 || oper > 15)return -1;
  char tmper[40];
  sprintf(tmper,"%d,%s",oper,psw);
  return SendCmdAll(hwnd,Fn,UI,104,tmper);
}




int       ReportModemStat(HWND hwnd, void ( *Fn), LPARAM UI, int prnt)
{
	sprintf(Tmp,"%i", prnt);
	return SendCmdAll(hwnd,Fn,UI,0x6c,Tmp);         //0x6c
}



int       ReadNetParams(HWND hwnd, void ( *Fn), LPARAM UI)
{
	return SendCmdAll(hwnd,Fn,UI,0x25,"");         //0x25
}



int        SetNetParams(HWND hwnd, void ( *Fn), LPARAM UI, int DHCP, char* IPAdrReg, char *IPMask, char* IPRouter, char* IPEqua, char* PortEqua, char* TimePer, char* RepTimePer)
{
	sprintf(Tmp,"%i,%s,%s,%s,%s,%s,%s,%s", DHCP, IPAdrReg, IPMask, IPRouter, IPEqua, PortEqua, TimePer, RepTimePer);
	return SendCmdAll(hwnd,Fn,UI,0x24,Tmp);         //0x24
}



int        ReadID_DEV(HWND hwnd, void ( *Fn), LPARAM UI)
{
	return SendCmdAll(hwnd,Fn,UI,0xFF,"");         //0xFF
}


int        ChangeProt(HWND hwnd, void ( *Fn), LPARAM UI, int prot)
{
	sprintf(Tmp,"%i",prot);
	return SendCmdAll(hwnd,Fn,UI, 0x51,Tmp);         //0x51
}


}
}

