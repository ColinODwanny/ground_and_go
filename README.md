# Ground & Go

Welcome to the **Ground & Go** repository.

This project is a cross-platform fitness and mental wellness application built with **.NET MAUI**. The app is designed to help users connect their emotional state to their physical activity by providing a flow that includes mood check-ins, journaling, and tailored mindfulness exercises before generating a workout or logging a rest day.

The application uses **Supabase** for its backend database services, handling everything from user data to workout and journal entry storage.

## Sprint 4 Changes

* Added the workout generation functionailty 😀😀😀😀
* Added the rest day full flow 😀😀
* Profile page logs now have all the correctly mapped data 😀
* Updated the workout display to handle different categories of workouts 😀
* Activity complete pop up is now functional when completing activities 😀😀😀😀
* Icons in Profile page instead of emojis. 
* Forced light mode so pages wouldn't get scrambled. 
* Created a button to delete an account. 😀😀😀
* Updated App Icon with a little color. 


## Sprint 3 Changes

----------------------------------------------------------------
* TO LOGIN, USE EMAIL sarah.lee@email.com AND A PASSWORD OF: 123
----------------------------------------------------------------


* Added all exercises to the database that clients needed for workouts 😀😀
* All workouts have been formatted and they're ready to be implemented for next sprint 😀

* Fixed Silent Database Failures: Implemented a new SQL trigger in Supabase (on_auth_user_created) to automatically add new sign-ups from auth.users to the public.members table. This fixed the jouranals/workoutID that weren't inserting to the database.

* Updated the models to use string for uuid types (log_id, member_id).
* Made the workout_id properly nullable (int?) in the WorkoutLog model to allow for rest day logs.

* Using the DailyProgressService to carry the log_id across all pages in the workout/rest flow.

* Cleaned Up App Navigation: Removed the confusing middle "Activity" tab.😀 Re-architected the entire "Begin Activity" and "Rest Day" flows into a hierarchical navigation stack.

* Fixed Authentication State Bug: Corrected the app's startup logic to ensure the BusinessLogic class (used by the Login page) shares the same singleton Database instance as the rest of the app.😀

* UI changes improving overall aesthetic and limiting inputs to correct inputs.

* Functional login logic that pulls information from Supabase to validate user authenticity. 

## Sprint 2 Changes

* Login page is now the first thing that appears when the app launches and signs you into home 😀
* Workout navigation flow is complete, depending on rest or workout day. 😀
* Logout button reverts you to the login page now
* Journal entry page is inserted before and after activity
* Data can be pulled from the Supabase Database and is properly mapping to our C# objects 😀
* Workout and Journal logs now pull data from the database 😀😀
* The workout and journal log pages presentation has been updated 😀
* The "View Exercise" button in the workout page now plays the corresponding instructional video from the database 😀
* The ability to pass user input as parameters to other pages has been implemented
* Signup page has been designed and navigates similar to the login page
