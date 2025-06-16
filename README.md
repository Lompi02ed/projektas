
 Master priima duomenis iš agentų, sujungia rezultatus ir pateikia išvestį. 
 ScannerA skenuoja katalogą `A`, analizuoja žodžius ir siunčia Master procesui.
 ScannerB veikia taip pat kaip ScannerA, bet dirba su kitu katalogu `B`.

---

istorija

 1. Pradinis - Sukurti trys projektai: Master, ScannerA, ScannerB 
 2. Named Pipes - Įgyvendinta komunikacija tarp procesų naudojant NamedPipeClient/Server 
 3. Žodžių skaičiavimas - ScannerA/B skenuoja .txt failus, skaičiuoja žodžius, siunčia JSON 
 4. Multithreading - Master procesas naudoja dvi gijas, kad vienu metu apdorotų agentus 
 5. CPU Affinity - Kiekvienas procesas priskirtas atskirai CPU šerdžiai 
 6. Testavimas - Vykdytas paleidimas per CMD su argumentais 

---

 "WordIndex.cs" yra bendras duomenų modelis, naudojamas JSON perdavimui tarp agentų ir Master.
 Named pipes veikia tik kai Master startuoja pirmas.
 CPU šerdys nustatomos su "ProcessorAffinity".


![image](https://github.com/user-attachments/assets/fad41ddd-8444-4439-919e-746c5a4b69d5)

