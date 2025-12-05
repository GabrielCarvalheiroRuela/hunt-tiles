# 🎮 Hunt Tiles - Jogo de Estratégia em Grid

<div align="center">

![Unity](https://img.shields.io/badge/Unity-6000.0+-white?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-Build-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![License](https://img.shields.io/badge/License-All%20Rights%20Reserved-red?style=for-the-badge)

**Um jogo roguelike em grid com sistema de níveis progressivos, coleta de itens, inimigos inteligentes e power-ups dinâmicos**

[Sobre](#-sobre-o-projeto) •
[Funcionalidades](#-funcionalidades) •
[Mecânicas](#️-mecânicas-do-jogo) •
[Tecnologias](#️-tecnologias) •
[Instalação](#-instalação) •
[Controles](#-controles) •
[Sistema de Jogo](#-sistema-de-jogo)

</div>

---

## 📖 Sobre o Projeto

**Hunt Tiles** é um jogo roguelike em 2D desenvolvido em **Unity C#** que combina elementos de estratégia, exploração e ação. O jogador controla um personagem em um tabuleiro dinâmico preenchido com:

- 🪙 **Moedas** para coletar e ganhar pontos
- ⚡ **Power-ups** com efeitos especiais
- 🧱 **Obstáculos** que bloqueiam o caminho
- 👹 **Inimigos** com IA inteligente de perseguição
- 📈 **10 Níveis** com dificuldade progressiva

### 🎯 Objetivo Principal

Colete todas as moedas em cada nível para avançar. Evite os inimigos ou use power-ups para se proteger. Complete todos os 10 níveis para vencer o jogo!

---

## ✨ Funcionalidades

### 🎮 Gameplay

| Funcionalidade | Descrição |
|---|---|
| **🗂️ Sistema de Grid** | Tabuleiro dinâmico com colisão e movimentação por célula |
| **🕹️ Controles Responsivos** | WASD + Setas + Clique do Mouse para movimento |
| **🪙 Coleta de Itens** | Sistema de coleta de moedas com feedback visual e sonoro |
| **⚡ Power-ups Dinâmicos** | Velocidade, Pontuação Dupla, Invencibilidade |
| **👹 IA Inimigos** | Perseguição inteligente com pathfinding adaptativo |
| **❤️ Sistema de Vidas** | 3 vidas com dano por colisão e invencibilidade temporária |

### 📊 Sistema de Pontuação

| Ação | Pontos |
|------|--------|
| Coletar Moeda | +10 pts (ou +20 com Power-up) |
| Completar Nível | Bônus de tempo + bônus de nível |
| Vencer Jogo | +1000 pts finais |

### 🎮 Modos de Jogo

- **🌟 Nível Progressivo**: 10 níveis com dificuldade crescente
- **🔄 Reintentar**: Reinicie o nível ou retorne ao menu
- **⏸️ Pausa**: Pause a qualquer momento com menu interativo

---

## 🛠️ Mecânicas do Jogo

### 📏 Sistema de Grid
- Tabuleiro dinâmico com células navegáveis
- Colisão com obstáculos e limites do mapa
- Verificação automática de posições válidas

### 👤 Personagem Jogável
- Movimento livre em 4 direções (WASD/Setas)
- Clique para movimento direto
- Animação suave com easing
- Feedback visual ao tomar dano

### 👹 Inimigos Inteligentes
- **Perseguição Adaptativa**: Escolhem melhor direção a cada movimento
- **Velocidade Progressiva**: Mais rápidos quando próximos ao jogador
- **Colisão de Inimigos**: Não atravessam uns aos outros
- **Cooldown de Dano**: Não causam dano infinito

### 💎 Power-ups

| Power-up | Efeito | Duração |
|----------|--------|---------|
| ⚡ **Velocidade** | Movimento 2x mais rápido | 10s |
| ✨ **Pontuação Dupla** | Moedas valem 2x | 10s |
| 🛡️ **Invencibilidade** | Imune a inimigos (inimigos desaparecem) | 10s |

### 🏆 Condições de Vitória/Derrota

**Vitória:**
- Coletar todas as moedas → Avança nível
- Completar 10 níveis → Jogo ganho

**Derrota:**
- Perder 3 vidas → Tela de Game Over

---

## 🎵 Sistema de Áudio

Sons procedurais gerados em tempo real:

| Som | Tipo | Situação |
|-----|------|----------|
| 🪙 Moeda | Bip agudo duplo | Coleta de moeda |
| ⚡ Power-up | Arpejo mágico | Ativação de power-up |
| 💥 Dano | Tom grave áspero | Recebimento de dano |
| 🏆 Vitória | Fanfarra (C-E-G-C) | Fim de nível/jogo |
| 😢 Derrota | Tons descendentes | Perda de jogo |
| 🎉 Nível | Arpejo maior (G-B-D-G) | Conclusão de nível |
| ⏸️ Pausa | Bip suave | Ativação de pausa |

**Volume:**
- Master: 15%
- Efeitos: 30%
- Música: 10%

---

## 🎨 Interface Gráfica

### Visual Premium
- 🌲 **Texturas de Madeira**: Estilo natural e profissional
- ✨ **Bordas Douradas**: Efeito de profundidade e elegância
- 🎭 **Animações Suaves**: Movimentação com easing curves
- 💫 **Feedback Visual**: Pulsação, brilhos, sombras

### HUD Responsivo
- **Posicionamento Dinâmico**: Adapta-se ao tamanho do tabuleiro
- **Painel Direito**: Status e pontuação
- **Painel Esquerdo**: Tutorial e dicas
- **Centro Superior**: Mensagens centralizadas

### Painéis Especiais
- 📖 **Tutorial**: Explicação completa de controles e objetivos
- ⏸️ **Pausa**: Menu com opções de continuar/reiniciar/menu
- 🏆 **Vitória**: Exibição de pontuação final
- 💀 **Derrota**: Informações de game over

---

## ️️ Tecnologias

### Engine
- **Unity 6000.0+** - Engine de desenvolvimento de jogos 3D/2D
- **C# 12.0** - Linguagem de programação principal

### Sistemas Principais
- **InputSystem** - Novo sistema de input da Unity
- **UI Toolkit** - Sistema de interface gráfica
- **Coroutines** - Gerenciamento de animações e eventos

### Padrões de Programação
- **Singleton Pattern** - Gerenciador único do jogo
- **Observer Pattern** - Sistema de eventos
- **State Machine** - Estados do jogo (Playing, Paused, GameOver)

### Arquitetura

```
┌─────────────────────────────────────────────┐
│           HUNT TILES - Arquitetura          │
├─────────────────────────────────────────────┤
│                                             │
│  ┌─────────────┐  ┌─────────────────────┐  │
│  │  Menu Scene │  │  Game Scene         │  │
│  │             │  │                     │  │
│  │ - Menu      │  │ ┌─────────────────┐ │  │
│  │ - Créditos  │  │ │ GerenciadorJogo │ │  │
│  │ - Opções    │  │ │ (Singleton)     │ │  │
│  │ - Áudio     │  │ └─────────────────┘ │  │
│  └─────────────┘  │                     │  │
│                   │ ┌──────────────────┐│  │
│                   │ │  Tabuleiro       ││  │
│                   │ │  (Grid System)   ││  │
│                   │ └──────────────────┘│  │
│                   │                     │  │
│                   │ ┌──────┐ ┌──────┐  │  │
│                   │ │Player│ │Enemy │  │  │
│                   │ └──────┘ └──────┘  │  │
│                   │                     │  │
│                   │ ┌────────────────┐  │  │
│                   │ │InterfaceJogo   │  │  │
│                   │ │(HUD/UI)        │  │  │
│                   │ └────────────────┘  │  │
│                   │                     │  │
│                   └─────────────────────┘  │
│                                             │
└─────────────────────────────────────────────┘
```

---

### Componentes Principais

| Arquivo | Responsabilidade |
|---------|-----------------|
| **GerenciadorJogo.cs** | Controle central, física, colisões e progressão de níveis |
| **Tabuleiro.cs** | Sistema de grid, células navegáveis, obstáculos |
| **Personagem.cs** | Entrada do jogador, movimentação, coleta de itens |
| **Inimigo.cs** | IA de perseguição, movimentação, colisão com jogador |
| **InterfaceJogo.cs** | HUD, painéis de UI, feedback visual |
| **Coletavel.cs** | Moedas e power-ups, sistema de coleta |
| **MenuController.cs** | Menu inicial, navegação de cenas |

---

## 🚀 Instalação

### Pré-requisitos

- [Unity 6000.0+](https://unity.com/download)
- [Git](https://git-scm.com/)
- Windows 10/11 (para build)

### Passo a Passo

1. **Clone o repositório**
```bash
git clone https://github.com/GabrielCarvalheiroRuela/hunt-tiles.git
cd hunt-tiles
```

2. **Abra no Unity**
- Abra o Unity Hub
- Clique em "Add" e selecione a pasta `hunt-tiles`
- Selecione a versão Unity 6000.0+

3. **Aguarde a importação**
- O Unity importará todos os assets
- Pode levar alguns minutos na primeira vez

4. **Execute o jogo**
- Abra a cena `Assets/Scenes/Menu/Menu.unity`
- Pressione ▶️ (Play) ou `Ctrl+P`

### Gerar Build

1. Vá em **File → Build Settings**
2. Selecione **Windows** como plataforma
3. Clique em **Build** e escolha uma pasta
4. Execute o `.exe` gerado

---

## 🎮 Controles

### Movimento
| Tecla | Ação |
|-------|------|
| **W** | Mover para cima |
| **A** | Mover para esquerda |
| **S** | Mover para baixo |
| **D** | Mover para direita |
| **↑↓←→** | Setas (alternativa) |
| **Clique do Mouse** | Mover para célula clicada |

### Interface
| Tecla | Ação |
|-------|------|
| **ESC** | Pausar/Retomar jogo |
| **Enter** | Confirmar ação |
| **Mouse** | Navegar menu |

---

## 📊 Sistema de Jogo

### Progressão de Níveis

```
Nível  | Moedas | Inimigos | Obstáculos | Dificuldade
-------|--------|----------|------------|-------------
  1    |   8    |    1     |     3      | ⭐
  2    |   10   |    2     |     4      | ⭐⭐
  3    |   12   |    2     |     5      | ⭐⭐
  4    |   14   |    3     |     6      | ⭐⭐⭐
  5    |   15   |    3     |     7      | ⭐⭐⭐
  6    |   18   |    4     |     8      | ⭐⭐⭐⭐
  7    |   20   |    4     |     9      | ⭐⭐⭐⭐
  8    |   22   |    5     |    10      | ⭐⭐⭐⭐⭐
  9    |   25   |    5     |    11      | ⭐⭐⭐⭐⭐
  10   |   30   |    6     |    12      | 🔥 MÁXIMO
```

### Fórmula de Pontuação

```
Pontos Base = Moedas Coletadas × 10

Se Poder Duplo Ativo:
  Pontos Moeda = Moedas Coletadas × 20

Bônus Tempo = max(0, 180 - Tempo em Segundos)
Bônus Nível = Nível Atual × 100

Pontuação Final = Pontos Base + Bônus Tempo + Bônus Nível
```

---

## 📄 Licença

⚠️ **TODOS OS DIREITOS RESERVADOS**

Este projeto está protegido por uma **licença proprietária restritiva**.

**É PROIBIDO** sem autorização prévia por escrito:
- Copiar ou reproduzir o código
- Modificar ou distribuir
- Usar para fins comerciais

Consulte o arquivo [LICENSE](LICENSE) para mais detalhes.

---

<div align="center">

Desenvolvido com ❤️ para fins academicos

*"Um desafio estratégico em cada nível"*

**[Voltar ao Topo](#-hunt-tiles---jogo-de-estratégia-em-grid)**

</div>
