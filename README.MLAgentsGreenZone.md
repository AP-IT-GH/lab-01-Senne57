# Sequentieel taakgedrag met Unity ML-Agents
Je vindt de code in scripts met de naam CubeAgentRaysWithGreenzone, en de scene noemt Labo03

## Overzicht
In dit project leert een agent twee acties na elkaar uitvoeren:
1. Een rode balletje zoeken en aanraken  
2. Daarna naar een groene zone gaan  

De episode eindigt zodra beide stappen voltooid zijn.

## Doel
Nagaan of een agent dit sequentieel gedrag kan aanleren binnen een beperkte trainingstijd.

## Omgeving
- Vlak speelveld  
- Agent start op het platform
- balletje verschijnt op een willekeurige positie  
- Groene zone ligt vast in de scene  
- Acties: vooruit bewegen en roteren  

## Beloningen
- Blokje aanraken: +0.5  
- Groene zone bereiken (na blokje): +1.0  
- Tijdstraf: −0.001 per stap  
- Episode stopt bij succes of wanneer de agent valt  

## Observaties
- Positie agent  
- Positie rode balletje  
- Positie groene zone  
- Status (blokje verzameld of niet)  

## Training
- Algoritme: PPO (Unity ML-Agents)  
- ~340.000 stappen  
- Standaard instellingen  
- Eén trainingsrun  

## Resultaten
- Cumulatieve beloning stijgt tot ~1.25 en bereikt een plateau  
- Episodelengte daalt van ~100 naar ~14 stappen  
- Value loss daalt, policy loss blijft schommelen  

## Conclusie
Het lijkt erop dat de agent het gewenste gedrag aanleert: eerst het blokje zoeken, daarna de zone bereiken.  
