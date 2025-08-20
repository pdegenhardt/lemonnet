#ifndef LEMON_CONFIG_H
#define LEMON_CONFIG_H

#define LEMON_VERSION "1.3.1"
#define LEMON_HAVE_LONG_LONG 1

/* We don't need LP/MIP solvers for Edmonds-Karp */
/* #define LEMON_HAVE_LP 1 */
/* #define LEMON_HAVE_MIP 1 */
/* #define LEMON_HAVE_GLPK 1 */
/* #define LEMON_HAVE_CPLEX 1 */
/* #define LEMON_HAVE_SOPLEX 1 */
/* #define LEMON_HAVE_CLP 1 */
/* #define LEMON_HAVE_CBC 1 */

#ifdef _WIN32
#define LEMON_USE_WIN32_THREADS 1
#else
#define LEMON_USE_PTHREAD 1
#endif

#endif // LEMON_CONFIG_H