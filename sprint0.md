# Project Proposal – Car Trading Card Game

# Table of Contents

1. [Project Summary](#project-summary)
2. [Features](#features)
   - [Core Features](#core-features)
   - ["Nice-to-Have" Features](#nice-to-have-features)
3. [User Stories](#user-stories)
4. [Initial Architecture](#initial-architecture)
   - [High-Level Architecture Diagram](#high-level-architecture-diagram)
   - [Planned Technologies](#planned-technologies)
   - [Architecture Rationale](#architecture-rationale)
5. [Work Division & Coordination](#work-division--coordination)

# Project Summary
Our project is a digital **Trading Car Card Game** where players collect, trade, and race cars represented as digital cards. Each car card has unique performance stats and rarity levels, making collection and strategy central to gameplay.  

The goal is to deliver both the excitement of opening randomized packs and the long-term engagement of completing collections, trading with friends, and racing cars competitively. By blending aspects of collectible card games with racing mechanics, our app provides a unique experience for both car enthusiasts and fans of card-based games.

You can also view the PDF copy of our presentation slides for this proposal here: [Cardex-Proposal-Slides.pdf](docs/Cardex-Proposal-Slides.pdf), or found in `/docs/Cardex-Proposal-Slides.pdf`

---

# Features

## Core Features

These are the features for our Minimum Viable Product (MVP) that are essential to exist at launch.

1. **Pack Opening** – Players open randomized packs to unlock cars of varying rarities.  
2. **Trading** – Secure peer-to-peer trading of car cards between players.  
3. **Collections** – Completing themed sets grants rewards like bonus packs or exclusive cars.  
4. **Racing** – Cars can race against other players or AI based on stats.  
5. **Scalability** – The system should be able to handle at least **100 concurrent users** performing core actions (pack opening, trading, racing) without degradation.

## "Nice-to-Have" Features

These are the features that are not necessary right away, but will be nice to have for user-experience.

6. **Upgrades** – Players upgrade performance stats or personalize cars with skins and decals.  
7. **Socials** – Players can showcase collections, rare pulls, and wins on leaderboards or in a community hub.

---

# User Stories

## [User Story 1: Account Creation](https://github.com/VSHAH1210/CarDex/issues/1)
**"As a new user, I want to create an account with username and password, so that I can login and start collecting and trading car cards"**

- [ ] User can register with a unique username and secure password
- [ ] Username must be unique across the platform
- [ ] User receives starting currency (e.g., 1000 coins) upon account creation
- [ ] System generates unique UUID for each user
- [ ] Error messages display for duplicate usernames or invalid passwords


## **[User Story 2: Daily Login Rewards](https://github.com/VSHAH1210/CarDex/issues/3)**
**"As a registered user, I want to earn currency by logging in daily, so that I can purchase packs without spending real money"**  

- [ ] User receives currency reward upon daily login (e.g., 100 coins)
- [ ] Currency is automatically added to user's account balance
- [ ] User can only claim one reward per 24-hour period
- [ ] System tracks last login timestamp to prevent multiple claims
- [ ] Balance updates are reflected immediately in the user interface
- [ ] Login streak bonuses apply for consecutive days (optional enhancement)
---

## **[User Story 3: Pack Purchase](https://github.com/VSHAH1210/CarDex/issues/4)**
**"As a collector, I want to buy packs using my earned currency, so that I can obtain new cards for my collection"**  

- [ ] User can browse available packs from different collections (JDM, Muscle, Supercars, etc.)
- [ ] Each pack displays its collection name, image, and currency cost
- [ ] User can only purchase packs if they have sufficient currency
- [ ] Currency is deducted from user balance upon successful purchase
- [ ] Purchased pack is added to user's "owned_packs" list
- [ ] Transaction fails gracefully with error message if insufficient funds
- [ ] Purchase confirmation is displayed to user

## **[User Story 4: Pack Opening](https://github.com/VSHAH1210/CarDex/issues/5)**
**"As a collector, I want to open packs I own, so that I can reveal new cards and add them to my garage"**  

- [ ] User can view all packs they own
- [ ] User can select and open any owned pack
- [ ] System randomly selects a vehicle from the pack's collection
- [ ] System randomly assigns a grade (FACTORY, LIMITED_RUN, or NISMO) based on rarity percentages
- [ ] New card is created with assigned vehicle, grade, and calculated value
- [ ] Card is added to user's "owned_cards" collection
- [ ] Pack is removed from user's "owned_packs" after opening
- [ ] Opening animation/reveal is displayed to user
- [ ] User cannot open the same pack twice

## **[User Story 5: Virtual Garage Management](https://github.com/VSHAH1210/CarDex/issues/6)**
**"As a collector, I want to view and organize my card collection, so that I can track my progress and showcase my best cards"**  

- [ ] User can view all cards they own in a gallery/list format
- [ ] Each card displays vehicle image, year, make, model, stats, grade, and value
- [ ] User can filter cards by collection, grade, manufacturer, or year
- [ ] User can sort cards by value, year, or alphabetically
- [ ] Card count and total collection value are displayed
- [ ] User can view detailed stats for individual cards
- [ ] Collection completion percentage is shown for each collection set

## **[User Story 6: Listing Card for Sale](https://github.com/VSHAH1210/CarDex/issues/7)**
**"As a seller, I want to list my cards on the marketplace for currency, so that I can earn coins to buy more packs"**  

- [ ] User can select a card from their collection to list for sale
- [ ] User can set a price in currency for the card
- [ ] System suggests a price based on card value and market trends (optional)
- [ ] Listed card creates an OPEN_TRADE record with type "FOR_PRICE"
- [ ] Listed card remains in user's collection but is marked as "in trade"
- [ ] User can view all their active listings
- [ ] User can cancel a listing at any time before it's purchased
- [ ] Multiple cards can be listed simultaneously

## **[User Story 7: Trading Cards](https://github.com/VSHAH1210/CarDex/issues/8)**
**"As a trader, I want to offer my card in exchange for another specific card, so that I can complete my collection through direct trades"**  

- [ ] User can select a card to offer for trade
- [ ] User can specify which card they want in return (by searching/browsing)
- [ ] System creates an OPEN_TRADE record with type "FOR_CARD"
- [ ] Both cards are marked as "in trade" and cannot be listed elsewhere
- [ ] Trade offer appears in marketplace filtered by "card trades"
- [ ] Owner of the wanted card can accept or decline the offer
- [ ] User can cancel their trade offer before it's accepted
- [ ] System validates both cards exist and are owned by correct users

## **[User Story 8: Completing Marketplace Transactions](https://github.com/VSHAH1210/CarDex/issues/9)**
**"As a buyer, I want to purchase cards or accept trade offers from the marketplace, so that I can acquire cards I need for my collection"**  

- [ ] User can browse all active marketplace listings (both currency sales and card trades)
- [ ] User can filter marketplace by collection, grade, price range, or trade type
- [ ] **For Currency Purchases:**
  - [ ] User can only purchase if they have sufficient currency
  - [ ] Currency is transferred from buyer to seller
  - [ ] Card ownership transfers from seller to buyer
- [ ] **For Card Trades:**
  - [ ] User can only accept if they own the requested card
  - [ ] Both cards transfer ownership simultaneously
  - [ ] System validates both users still own their respective cards
- [ ] COMPLETED_TRADE record is created with all transaction details
- [ ] OPEN_TRADE listing is removed from marketplace
- [ ] Both users receive confirmation of completed transaction
- [ ] Trade history is recorded with execution date and details
- [ ] Transaction fails gracefully if conditions aren't met (insufficient funds, card no longer available)

# Initial Architecture

## High-Level Architecture Diagram
![High Level Architecture Diagram](docs/High-Level-Architecture-Diagram.png "High Level Architecture Diagram")

## Planned Technologies

Frontend (Mobile): **Flutter**

Frontend (Web): **React**

Backend API: **ASP.NET Core** (or Fastify/Node.js as fallback)

Database: **PostgreSQL + Entity Framework Core (EF Core)**

Deployment/DevOps: **Docker**

### Architecture Rationale

This architecture works well because it separates concerns cleanly between mobile, web, backend, and database tiers.  

- **Flutter (Mobile):** One codebase for iOS and Android with near-native speed, ideal for swipe, flip, and trading animations that make the mobile garage feel interactive and alive.  
- **React (Web):** Great for building rich, responsive websites; it lets us show high-resolution car cards and makes comparing collections on desktop smooth and fast.  
- **ASP.NET Core / Fastify (Backend API):** Provides a secure, high-performance backend that keeps trades instant, updates collections in real time, and scales as more collectors join.  
- **Swagger / OpenAPI:** Enables live, interactive API documentation so web and mobile teams can test trading and racing features without guesswork.  
- **PostgreSQL + Entity Framework Core (Database):** A rock-solid database to store every car card, user garage, and trade history safely, while Entity Framework Core simplifies database interactions and provides a robust migration system for schema evolution.
- **Docker:** Ensures consistent development environments and simplifies local testing and future deployment.  

# Work Division & Coordination
    
With 6 team members, we will divide work across three main areas:

Frontend (Flutter & React): 2 members will focus on mobile and web UI/UX.

Backend (API & Logic): 2 members will implement API endpoints, authentication, and racing/trading logic.

Database & DevOps: 2 members will design schemas, manage migrations with Entity Framework Core, and handle Docker setup/deployment.


We will coordinate work using weekly meetings, short sprint cycles, and GitHub pull requests with code reviews. Communication will be managed through a team chat (e.g., Discord/Slack) and task tracking in GitHub Projects or Jira.
