#define WM_FPRESPONSE  0x1099
#define PREAMBLE      0x01
#define NACK          0x15
#define SYN           0x16
#define TERMINATOR    0x03
#define POSTAMBLE     0x05
#define NOPAPER       0x13

#define SYNTAX_ERROR                  0x00000001
#define INVALID_CMD                   0x00000002
#define INVALID_TIME                  0x00000004
#define PRINT_ERROR                   0x00000008
#define SUM_OVERFLOW                  0x00000010
#define CMD_NOT_ALLOWED               0x00000020
#define RAM_CLEARED                   0x00000040
#define PRINT_RESTART                 0x00000080
#define RAM_DESTROYED                 0x00000100
#define PAPER_OUT                     0x00000200
#define FISCAL_OPEN                   0x00000400
#define NONFISCAL_OPEN                0x00000800
#define SERVICE_OPEN                  0x00001000
#define F_ABSENT                      0x00002000
#define F_MODULE_NUM                  0x00004000
#define F_WRITE_ERROR                 0x00010000
#define F_FULL                        0x00020000
#define F_READ_ONLY                   0x00040000
#define F_CLOSE_ERROR                 0x00080000
#define F_LESS_30                     0x00100000
#define F_FORMATTED                   0x00200000
#define F_FISCALIZED                  0x00400000
#define F_SER_NUM                     0x00800000

#define PROTOCOL_ERROR                0x01000000
#define NACK_RECEIVED                 0x02000000
#define TIMEOUT_ERROR                 0x04000000
#define COMMON_ERROR                  0x08000000
#define F_COMMON_ERROR                0x10000000
#define ADD_PAPER                     0x20000000

#define ANY_ERROR                     0xff000000

ref struct  RetData {
               int Count;
               int CmdCode;
               LPARAM UserData;
               LPARAM Status;
               LPSTR CmdName;
               LPSTR SendStr;
               LPSTR Whole;
               LPSTR RetItem[20];
               unsigned char OrigStat[6];
               unsigned char RSD[300];
   				unsigned char RetDatX[300];
               };

 int enum  RcvStates { START,LEN,SEQ,CMD,DATA,SEP,STAT,EOT,CHKSUM,CTRC };


ref struct  Answer
	{
	unsigned char Status[6];
	unsigned char Seq;
	unsigned char Cmd;
	char Data[300];
	unsigned char ReadyFlag;
	unsigned char RecError;
	unsigned char Len;
	unsigned CalcSum;
	unsigned CheckSum;
   unsigned char RSD[300];
   unsigned char RetDatX[300];
	};
	
	
 unsigned char  DisplayTable[128] = {
    0x20,0x20,0x20,0x20,0x22,0x20,0x20,0x20, // 80h
    0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,
    0x20,0x27,0x27,0x22,0x22,0x20,0x20,0x20, // 90h
    0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,
    0x20,0x20,0x20,0x20,0x20,0xA5,0x20,0x20, // A0h
    0x20,0x20,0xAA,0x20,0x20,0x20,0x20,0xD8,
    0x20,0x20,0x49,0x69,0xB4,0x20,0x20,0x20, // B0h
    0x20,0x20,0xBA,0x20,0x20,0x20,0x20,0x8C,
    0x41,0xB0,0x42,0xB1,0xB2,0x45,0xB3,0x33, // C0h
    0xB8,0xB9,0x4B,0xBA,0x4D,0x48,0x4F,0xBB,
    0x50,0x43,0x54,0xBC,0xBF,0x58,0xC0,0xC1, // D0h
    0xC2,0xC3,0xC4,0xC5,0x62,0xC8,0xC9,0xCA,
    0x41,0xB0,0x42,0xB1,0xB2,0x45,0xB3,0x33, // E0h
    0xB8,0xB9,0x4B,0xBA,0x4D,0x48,0x4F,0xBB,
    0x50,0x43,0x54,0xBC,0xBF,0x58,0xC0,0xC1, // F0h
    0xC2,0xC3,0xC4,0xC5,0x62,0xC8,0xC9,0xCA };

static unsigned char table[256]=
{
0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,

0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0x85, 0xA9, 0xC0, 0xAB, 0xAC, 0xAD, 0xAE, 0xC2,
0xB0, 0xB1, 0x49, 0x69, 0xB4, 0xB5, 0xB6, 0xB7, 0xA5, 0xB9, 0xC1, 0xBB, 0xBC, 0xBD, 0xBE, 0xC3,
0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F,
0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F,
0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF,
0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF
};

static unsigned char table1[256]=
{
0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F,
0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F,
0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F,
0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F,
0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F,
0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F,

0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,
0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xC0, 0xAB, 0xAC, 0xAD, 0xAE, 0xC2,
0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,
0xAA, 0xBA, 0xAF, 0xBF, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF,
0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF,
0xE0, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF,
0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF
};


LPSTR NotValidCmd= "Неверная команда";
LPSTR CmdNames[]= {
		 "Очистка дисплея",                                 	// 33
		 "",
		 "Дисплей - нижний ряд",                              // 35
       "Установка параметров связи с эквайером",            // 36
       "Чтение параметров связи с эквайером",               // 37
		 "Открыть нефискальный чек",                          // 38
		 "Закрыть нефискальный чек",                          // 39
		 "","",
		 "Печать нефискального текста",                 		// 42
		 "Установить Header, Footer и опции печати",       	// 43
		 "Пропустить строки на принтере",                     // 44
		 "Отрезать чек",													// 45
       "",
		 "Дисплей - верхний ряд",                             // 47
		 "Открыть фискальный чек",                            // 48
       "",
//		 "Регистрация продажи",                               // 49   ******
		 "Печать налоговых ставок за период",                 // 50   ******
		 "Подсумма",                                          // 51
		 "Регистрация продажи  и вывод на дисплей",           // 52
		 "Общая сумма чека (Total)",                          // 53
		 "Печать строк в фискальном чеке",                   	// 54
       "",																  	// 55
//		 "Печать информации о текущем фиск. чеке",  				// 55   ******
		 "Закрыть фискальный чек",                            // 56
		 "Аннулировать чек",												// 57
       "Регистрация продажи",                               // 58
       "","",
		 "Установить дату и время",                           // 61
		 "Получить дату и время",                           	// 62
		 "Вывести дату и время на дисплее",                   // 63
		 "Информация о последнем Z-отчете",           			// 64
		 "Суммы за день",                      					// 65   ******
		 //"Просмотреть фискальную память",                   // 66
       "",
		 "Суммы коррекций за день",                     		// 67
		 "Размер фискальной памяти",                          // 68   ******
		 "X- или Z- отчет",                                   // 69   ******
		 "Служебный внос/вынос суммы",                        // 70
		 "Печать диагностической информации",                 // 71
		 "Фискализация",                                      // 72
		 "Полный периодический отчет по номеру", 					// 73
		 "Получить состояние регистратора",                   // 74
		 "",                              							// 75
		 "Состояние фискальной транзакции",    					// 76
		 "",                            								// 77
		 "",
		 "Сокращенный периодический отчет (по дате)",         // 79
		 "Звуковой сигнал",       										// 80
       "Переключить протокол",                              // 81
       "",
		 "Установить множитель, запятую, валюту и количество групп налогов",  // 83
		 "Режим продаж",            									// 84
		 "Открыть чек возврата",										// 85
       "",
		 "",            													// 87
		 "",               												// 88
		 "Программирование тестовой области",         			// 89
		 "Запрос диагностической информации",                 // 90  ******
		 "Установить заводской номер", 								// 91
		 "Установить фискальный номер", 								// 92
       "",
		 "Полный периодический отчет по дате",  					// 94
		 "Сокращенный периодический отчет по номеру",    		// 95  ******
		 "",                      										// 96
		 "Ставки налогов",                         				// 97
		 "Установить налоговый или иденификационный номер",   // 98
		 "Прочитать налоговый или иденификационный номер",    // 99
		 "Управление дисплеем",                               // 100      ******
		 "Задать пароль оператора",									// 101
       "Задать имя оператора",                              // 102
       "Информация о текущем чеке",                         // 103
       "Обнулить данные оператора",                         // 104
       "Отчет оператора",												// 105
       "Открыть денежный ящик",										// 106
       "Программирование артикулов и получение информации о них", //107
       "Запрос статуса модема",                             // 108
       "Печать копии чека",											// 109
		 "Дополнительная информация по типам оплаты",         // 110
		 "Отчет товаров" , 										      // 111
		 "Информация об операторе" ,									// 112
		 "Номер последнего чека" , 									// 113
		 "Получение информации из фискальной памяти" ,			// 114
		 "Загрузка логотипа"  ,											// 115
       "Печать копии фискального документа",                // 116
       "",
		 "Пароль администратора" , 									// 118
		 "Обнулить операторские пароли" };						   // 119

char ver[100]="Ver. 2.0 28.02.06";
#define HANDLE_WM_FPRESPONSE(hwnd, wParam, lParam, fn) \
                ((fn)((hwnd), (wParam), (struct RetData far *)(lParam)), 0L)
/*
extern "C" {

//Windows95 5.10.98
HANDLE CALLBACK InitFPport(int, int);

int   CALLBACK CloseFPport(void);
int   CALLBACK SendCmdMsg(HWND, LPARAM, int, LPSTR);
int   CALLBACK SendCmdCbk(void CALLBACK far *, long, int, LPSTR);
int CALLBACK SendCmdAll(HWND hwnd,void (CALLBACK *Fn),LPARAM UI,int Cmd,LPSTR Data);

int   CALLBACK SendLast(BOOL);
LPSTR CALLBACK CmdDescription(int);
void  CALLBACK SetMessageNum(UINT);
int   CALLBACK SetDecimals(int);
int CALLBACK  SetShowAmount(int);

int   CALLBACK OpenNonfiscalReceipt(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK CloseNonfiscalReceipt(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK PrintNonfiscalText(HWND, void (CALLBACK far *), LPARAM, LPSTR);
int   CALLBACK SetHeaderFooter(HWND, void (CALLBACK far *), LPARAM, int, LPSTR);
int   CALLBACK AdvancePaper(HWND, void (CALLBACK far *), LPARAM, int);
int   CALLBACK DisplayTextLL(HWND, void (CALLBACK far *), LPARAM, LPSTR);
int   CALLBACK ClearDisplay(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK DisplayTextUL(HWND, void (CALLBACK far *), LPARAM, LPSTR);
int CALLBACK  OpenFiscalReceipt(HWND ,void (CALLBACK far*),LPARAM, DWORD,	LPSTR, DWORD, BOOL);
int   CALLBACK GetTaxRates(HWND, void (CALLBACK far *), LPARAM, LPSTR,LPSTR, LPSTR);
int CALLBACK SubTotal(HWND,void (CALLBACK far*),LPARAM,BOOL,BOOL, double, double);
int   CALLBACK Total(HWND, void (CALLBACK far *), LPARAM, LPSTR, char, double);
int   CALLBACK PrintFiscalText(HWND, void (CALLBACK far *), LPARAM, LPSTR);
int   CALLBACK CloseFiscalReceipt(HWND, void (CALLBACK far *), LPARAM);
//анулировать фискальный чек ResetReceipt (новая команда)  +++++++
int   CALLBACK ResetReceipt(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK SetDateTime(HWND, void (CALLBACK far *), LPARAM, LPSTR, LPSTR);
int   CALLBACK GetDateTime(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK DisplayDateTime(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK LastFiscalClosure(HWND, void (CALLBACK far *), LPARAM, int);
int   CALLBACK GetCurrentTaxes(HWND, void (CALLBACK far *), LPARAM, int);
int   CALLBACK GetCurrentSums(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK GetFreeClosures(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK FiscalClosure(HWND, void (CALLBACK far *), LPARAM, LPSTR, char);
int   CALLBACK ServiceInputOutput(HWND, void (CALLBACK far *), LPARAM, double);
int   CALLBACK PrintDiagnosticInfo(HWND, void (CALLBACK far *), LPARAM);
//Fiscalise - сильно изменена
int   CALLBACK Fiscalise(HWND, void (CALLBACK far *), LPARAM, LPSTR, LPSTR, LPSTR, int);
//PrintFiscalMemoryByNum доработать исходный текст
int   CALLBACK PrintFiscalMemoryByNum(HWND, void (CALLBACK far *), LPARAM, LPSTR, int, int);
int   CALLBACK GetStatus(HWND, void (CALLBACK far *), LPARAM, BOOL);
int   CALLBACK GetFiscalClosureStatus(HWND, void (CALLBACK far *), LPARAM, BOOL);
//PrintReportByDate доработать исходный текст
int   CALLBACK PrintReportByDate(HWND, void (CALLBACK far *), LPARAM, LPSTR, LPSTR, LPSTR);
int   CALLBACK PrinterBeep(HWND, void (CALLBACK far *), LPARAM);
//SetMulDecCurRF проверить
int CALLBACK  SetMulDecCurRF(HWND hwnd,void (CALLBACK *Fn),LPARAM UI,
		LPSTR psw, int Dec,LPSTR enabled, double taxA, double taxB, double taxC, double taxD);
int   CALLBACK GetMulDecCurRF(HWND, void (CALLBACK far *), LPARAM);

int   CALLBACK OperatorDataNull(HWND, void (CALLBACK far *), LPARAM, int, LPSTR);



//SetTaxType - новая команда
int   CALLBACK SetTaxType(HWND, void (CALLBACK far *), LPARAM, int);
//OpenRepaymentReceipt - новая команда
int CALLBACK  OpenRepaymentReceipt(HWND ,void (CALLBACK far*),LPARAM, DWORD,	LPSTR, DWORD, BOOL);
int   CALLBACK ProgramTestArea(HWND, void (CALLBACK far *), LPARAM, BOOL);
int   CALLBACK GetDiagnosticInfo(HWND, void (CALLBACK far *), LPARAM, BOOL);
int   CALLBACK SetCountrySerial(HWND, void (CALLBACK far *), LPARAM, int, LPSTR);
int   CALLBACK SetFiscalNumber(HWND, void (CALLBACK far *), LPARAM, LPSTR);
int   CALLBACK PrintFiscalMemoryByDate(HWND, void (CALLBACK far *), LPARAM, LPSTR, LPSTR, LPSTR);
int   CALLBACK PrintReportByNum(HWND, void (CALLBACK far *), LPARAM,LPSTR, int, int);
int   CALLBACK GetCurrentTaxRates(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK GetLastTaxRates(HWND, void (CALLBACK far *), LPARAM, LPSTR);
int   CALLBACK SetTaxNumber(HWND, void (CALLBACK far *), LPARAM,LPSTR, int);
int   CALLBACK GetTaxNumber(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK DisplayFreeText(HWND, void (CALLBACK far *), LPARAM, LPSTR);
int   CALLBACK SetOperatorPassword(HWND, void (CALLBACK far *), LPARAM, int NumOper, LPSTR OldPass, LPSTR NewPass);
int   CALLBACK SetOperatorName(HWND, void (CALLBACK far *), LPARAM, int NumOper, LPSTR Password, LPSTR Nane);
int   CALLBACK GetReceiptInfo(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK OpenDrawer(HWND, void (CALLBACK far *), LPARAM, int);
int   CALLBACK OperatorsReport(HWND, void (CALLBACK far *), LPARAM, LPSTR);
int   CALLBACK ArticulsReport(HWND, void (CALLBACK far *), LPARAM, LPSTR, char);
int   CALLBACK MakeReceiptCopy(HWND, void (CALLBACK far *), LPARAM, char Count);
int   CALLBACK DayInfo(HWND, void (CALLBACK far*), LPARAM);
int   CALLBACK GetOperatorInfo(HWND hwnd, void (CALLBACK far *Fn), LPARAM UI, int Operator);
int   CALLBACK GetLastReceipt(HWND hwnd, void (CALLBACK far *Fn), LPARAM UI);


int CALLBACK SaleArticle(HWND ,void (CALLBACK far*),LPARAM UI,
					 bool sign, int numart, double qwant, double perc, double  dc);
int CALLBACK SaleArticleAndDisplay(HWND ,void (CALLBACK far*),LPARAM UI,
					 bool sign, int numart, double qwant, double perc, double  dc);





//int   CALLBACK GetTaxRates(HWND, void (CALLBACK far *), LPARAM, LPSTR, LPSTR);
//??????
int   CALLBACK FiscalMemoryLookup(HWND, void (CALLBACK far *), LPARAM, int);




//FP3530T only
int   CALLBACK CutReceipt(HWND, void (CALLBACK far *), LPARAM);




//Windows95 5.10.98
HANDLE  CALLBACK GetRSId(void);

//int   CALLBACK GetRSId(void);
int   CALLBACK GetCommonArticleInfo(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK ProgrammingArticle(HWND, void (CALLBACK far *), LPARAM, char, int, int, double, LPSTR, LPSTR);
int   CALLBACK DeleteArticle(HWND, void (CALLBACK far *), LPARAM, int, LPSTR);
int   CALLBACK ChangeArticlePrice(HWND, void (CALLBACK far *), LPARAM, int, double, LPSTR);
int   CALLBACK GetFirstFreeArticleNum(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK GetArticleInfo(HWND, void (CALLBACK far *), LPARAM, int);
int   CALLBACK GetFirstArticleNum(HWND, void (CALLBACK far *), LPARAM);
int   CALLBACK GetNextArticleNum(HWND, void (CALLBACK far *), LPARAM);

int   CALLBACK LogoLoad(HWND, void (CALLBACK far *), LPARAM, LPSTR, int, char *);
int   CALLBACK SetAdminPassword(HWND, void (CALLBACK far *), LPARAM, LPSTR, LPSTR);
int   CALLBACK ClearOperatorPassword(HWND, void (CALLBACK far *), LPARAM, int, LPSTR);


int CALLBACK SetWinDosPage(bool);
bool CALLBACK GetWinDosPage();

//new modem fuctions - begin



int   CALLBACK    ReportModemStat(HWND hwnd, void (CALLBACK *Fn), LPARAM UI, int prnt);
int   CALLBACK    ReadNetParams(HWND hwnd, void (CALLBACK *Fn), LPARAM UI);
int   CALLBACK     SetNetParams(HWND hwnd, void (CALLBACK *Fn), LPARAM UI, int DHCP, char* IPAdrReg, char *IPMask, char* IPRouter, char* IPEqua, char* PortEqua, char* TimePer, char* RepTimePer);
int   CALLBACK     ReadID_DEV(HWND hwnd, void (CALLBACK *Fn), LPARAM UI);
int   CALLBACK     ChangeProt(HWND hwnd, void (CALLBACK *Fn), LPARAM UI, int prot);

//new modem fuctions - end

}
*/