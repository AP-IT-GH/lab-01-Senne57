# Sequentieel Taakgedrag met Unity ML-Agents
Je vindt de code in scripts met de naam ObelixAgent, en de scene noemt Labo04

## Overzicht
In dit project leert een agent Obelix een menhir ophalen en afleveren bij een bestemming. Bij meerdere menhirs herhaalt de agent deze cyclus tot alle menhirs afgeleverd zijn.

## Doel
Nagaan of een agent sequentieel gedrag kan aanleren: eerst een menhir ophalen, daarna naar de juiste bestemming gaan, en dit herhalen voor meerdere menhirs.

## Omgeving
- Vlak platform, agent start in het midden
- Menhirs en bestemmingen spawnen willekeurig op een cirkel (radius 9, min. 30° apart)
- Acties: vooruit bewegen en roteren (continue acties)
- Curriculum: training startte met 1 menhir, daarna opgeschaald naar 2, 3 en 5

## Beloningen
| Gebeurtenis | Beloning |
|---|---|
| Menhir oppakken | +0.5 |
| Menhir afleveren | +1.0 |
| Alle menhirs afgeleverd (bonus) | +2.0 |
| Tijdstraf per stap | -0.0001 |
| Afstandsbeloning per stap | +/- 0.01 x verandering in afstand |
| Tweede menhir aanraken (al bezet) | -3.0 + einde episode |
| Bestemming aanraken zonder menhir | -0.1 |
| Van het platform vallen | -1.0 + einde episode |

## Observaties
De observatievector bestaat uit 10 floats.

| Index | Observatie | Actief wanneer |
|---|---|---|
| [0] | hasMenhir (0 of 1) | Altijd |
| [1-3] | Richting naar dichtstbijzijnde menhir | hasMenhir = false |
| [4-6] | Richting naar dichtstbijzijnde bestemming | hasMenhir = true |
| [7-9] | Huidige snelheid (x, y, z) | Altijd |

## Problemen
- Obelix pakt tweede menhir terwijl hij er al een vasthoudt -> opgelost met straf -3.0 en onmiddellijk einde episode
- Obelix gaat naar bestemming zonder menhir -> opgelost via -0.1 straf en context-afhankelijke observaties

## Training
- ~1000000 steps
- PPO, learning rate 0.0003, batch size 64, hidden units 128
- Curriculum: 1 -> 2 -> 3 -> 5 menhirs

## Resultaten
- Cumulatieve beloning stijgt van ~1.3 (stap 500k) naar ~8.5 (stap 950k)
- Episodelengte daalt initieel sterk, stijgt licht bij meer menhirs (logisch door langere cycli)
<img width="1176" height="905" alt="image" src="https://github.com/user-attachments/assets/0d9e1eba-c1d5-4f92-9f44-cea5765160d4" />


## Conclusie
De agent leert het gewenste sequentieel gedrag succesvol aan: menhirs ophalen en afleveren in de juiste volgorde. Dankzij curriculum learning en context-afhankelijke observaties generaliseert het gedrag van 1 naar 5 menhirs. De cumulatieve beloning stijgt consistent, waardoor het lijkt dat de agent de taak beheerst.
