# Hunter VS Prey
Je vindt de code in scripts met de naam HunterAgent, PreyAgent en EnvironmentManager, en de scene noemt Hunter

## Overzicht
In dit project leren twee agents tegelijkertijd een tegengesteld doel te bereiken:
1. De Hunter moet de Prey opsporen en aanraken
2. De Prey moet rode blokken verzamelen zonder gepakt te worden door de Hunter

De episode eindigt wanneer de Hunter de Prey pakt, één van beiden de muur raakt, of de Prey alle blokken verzamelt.

## Doel
Het doel voor de hunter is de prey te pakken en het doel voor de prey is de hunter ontwijken en de blokken proberen op te pakken zonder gepakt te worden. Ze moeten beide de muur ook ontwijken.

## Omgeving
- Blauw vlak platform met rode muren als begrenzing
- 4 training areas draaien parallel tijdens training
- Hunter en Prey starten op willekeurige veilige posities
- 5 rode blokken spawnen willekeurig per episode

## Beloningen

### Hunter
| Gebeurtenis | Beloning |
|---|---|
| Prey aanraken | +3.0 |
| Muur raken | -2.0 |
| Tijdstraf per stap (oplopend) | -0.001 - (0.001 × stap/1000) |
| Dichterbij Prey komen | +0.001 × (10 - afstand) |

### Prey
| Gebeurtenis | Beloning |
|---|---|
| Rood blok oppakken | +1.0 |
| Alle blokken verzameld (bonus) | +3.0 |
| Gepakt worden door Hunter | -2.0 |
| Muur raken | -2.0 |
| Tijdstraf per stap (oplopend) | -0.001 - (0.0005 × stap/1000) |
| Afstand bewaren van Hunter | +0.0005 × afstand tot Hunter |

## Observaties

### Hunter (9 floats)
| Index | Observatie |
|---|---|
| [0-2] | Eigen positie (x, y, z) |
| [3-5] | Positie van Prey (x, y, z) |
| [6-8] | Eigen snelheid (x, y, z) |

### Prey (12 floats)
| Index | Observatie |
|---|---|
| [0-2] | Eigen positie (x, y, z) |
| [3-5] | Positie van Hunter (x, y, z) |
| [6-8] | Eigen snelheid (x, y, z) |
| [9-11] | Positie van dichtstbijzijnde rood blok (x, y, z) |

## Training
- ~500k stappen
- Standaard instellingen die ik gekregen heb in het word document
- beste run = run3Hunter
- 4 parallelle omgevingen

## Resultaten
- Hunter: cumulatieve beloning stijgt snel naar ~4.5 en stabiliseert rond **3.3**
- Prey: cumulatieve beloning zakt weg naar bijna **0 (~0.24)**
- Episodelengte daalt zeer snel naar een minimum → de Hunter pakt de Prey razendsnel
- De reward-histogram van de Hunter toont een duidelijke piek rond 3–5, wat volgens mij consistent gedrag aangeeft
- De Prey slaagt er nauwelijks in blokken te verzamelen voor ze gepakt wordt

<img width="1138" height="870" alt="image" src="https://github.com/user-attachments/assets/605f0ed8-f192-48a5-98f7-60a9cffbe0f8" />

## Problemen
- De Prey leert eigenlijk niet goed ontwijken en wordt te snel gevangen door de hunter
- Doordat de episodelengte zo kort is omdat de prey te snel gevangen word, heeft de Prey onvoldoende tijd om blokken te verzamelen

## Mogelijke verbeteringen
- **Spawn afstand**: De hunter verder laten spawnen van de Prey zodat die meer tijd heeft (momenteel spawnen ze niet heel ver van elkaar weg).
- **Curriculum learning**: ik zou voor de eerste 100K stappen de speed van de hunter zeer laag kunnen zetten en naarmate hoe meer stappen ze doen de speed hoger zetten zodat de prey beter kan leren.

## Conclusie
De Hunter leert zijn taak duidelijk aan: hij pakt de Prey consistent en snel. De Prey slaagt er niet in een effectieve ontwijkstrategie te ontwikkelen. volgens mij is het moeilijk om beide agents even goed te laten presteren omdat er altijd 1 van de 2 beter zal zijn.
