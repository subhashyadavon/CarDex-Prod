# Sprint 2 - Worksheet

</br>

## 1. Regression Testing


</br>
</br>

## 2. Testing Slowdown


</br>
</br>

## 3. Not Testing
The following parts of the code haven't been tested:
| Component | Tested | Untested | Coverage | Testing Level |
|----------|----------|----------|----------|----------|
| Frontend Services | 0/6 | 6/6 | 0% | Not tested |
| Frontend Context | 1/3 | 1/3 | 33% | Mostly tested |

The frontend coverage report can be found in CarDex\CarDexFrontend\coverage\lcov-report\index.html

</br>
</br>

## 4. Profiler

![Profiler Output Table](docs/Sprint2_API_Profiling.png)

`GET /cards` is the slowest endpoint. It currently returns all cards in a single response, which means latency will only grow as we add more cards. 

A potential fix is to add pagination to the response so that we are only returning the cards in batches as they get requested.  

</br>
</br>

## 5. Last Dash
 - We anticipate scope creep. As the application functions are getting bigger and complex than expected, we may not be able to complete all the targets that we aimed for at the start of the course.
 

</br>
</br>

## 6. Show Off

Ansh - https://github.com/VSHAH1210/CarDex/commit/5c564ca85360b074db41b05ca92e52e30bda63c0
 - The above commit is what I am most proud of. The thinking and brainstorming of how I could do this was interesting, frustrating and exciting to see it work.  I learnt so much about frontend as I have not worked this deeply in frontend before so I wasn’t an expert but understanding why we need some of these code files and techniques was interesting to learn and understand.

 Vansh - https://github.com/VSHAH1210/CarDex/commit/ff46a374c5c48cfda9cf0845d260fae19392c76c
- I’m most proud of creating our app’s registration page, my first time ever building something in React, and I’m proud of it because I stepped into a brand-new framework, owned the whole flow from start to finish, and delivered a simple, welcoming experience that helps people get into the app without friction. It unlocked onboarding for our team, reduced confusion for new users, and set a clear, consistent pattern we can reuse for future forms. Seeing my group members use it confidently was a big win and showed me I can learn fast, make thoughtful design choices, and ship work that feels polished and high-impact.

Ian – [feat(localization): add centralized .resx-based string localization for API and Services](https://github.com/VSHAH1210/CarDex/commit/1ad9b4b6394970cbb70a302a103d50cc20e1d660)

- I’m proud of setting up string localization with SharedResources.resx. It cleaned up a bunch of hardcoded strings across the backend and made everything way easier to manage, and will make it easier going forward. It felt great seeing the API return proper localized messages, and it sets us up nicely if we ever want to support multiple languages later on.

Jotham - [Created BrowserRoutes for Navigation, Garage, and PackShop; modified App.tsx](https://github.com/VSHAH1210/CarDex/commit/e304a5331d5c655e908e16dc44a4f48b23267245)

- The commit I’m most proud of is when I created the BrowserRoutes and wired together the main page components — Navigation, Garage, and PackShop — through App.tsx. This involved connecting multiple components, managing how data flowed through props, and ensuring everything rendered dynamically from our mock JSON files. It was challenging to structure the app so each component communicated smoothly while maintaining clean, modular code, but seeing the pages interact seamlessly felt like bringing the whole frontend to life. It taught me a lot about how React routing, props, and component composition work together to create a fully functional, scalable interface.

</br>
</br>
