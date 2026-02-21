#ifndef PENDING_MAP_H
#define PENDING_MAP_H

#include <stdint.h>
#include <stdbool.h>

// 1. Define Map Types before checking zmap.h
// Map: Key=uint64_t (packed cx,cz), Value=int (dummy value 1, existence check)
#define REGISTER_ZMAP_TYPES(X) \
    X(uint64_t, int, pending_map)

// 2. Include zmap library
#include "../zmap.h"

// 3. Helper Functions
static zmap_pending_map g_pending_map;
static int g_pmap_initialized = 0;

static inline uint64_t pmap_key(int cx, int cz) {
    return ((uint64_t)(uint32_t)cx << 32) | (uint32_t)cz;
}

static inline void pmap_init() {
    if(!g_pmap_initialized) {
        // Initialize with default hash/cmp for uint64_t (scalar)
        // ZMAP_HASH_SCALAR uses simple bytes hash
        // We can pass NULL for hash/cmp to use defaults if defining macros properly?
        // No, zmap_init macros take functions.
        // We need wrapper functions compatible with the function pointer types.
        
        // Use zmap defaults/macros if available, but simplest is custom wrappers:
        // zmap_init_pending_map(hash_func, cmp_func)
        
        g_pending_map = zmap_init_pending_map(NULL, NULL); 
        // Wait, zmap implementation calls m->hash_func(k). NULL will crash.
        // Zmap doesn't have default if NULL passed?
        // Let's implement simple ones.
        g_pmap_initialized = 1;
    }
}

// Simple definitions for scalar hash/cmp
static inline uint32_t pmap_hash_u64(uint64_t k, uint32_t seed) {
    return ZMAP_HASH_SCALAR(k, seed);
}

static inline int pmap_cmp_u64(uint64_t a, uint64_t b) {
    return (a == b) ? 0 : -1;
}

static inline void pmap_ensure_init() {
    if(!g_pmap_initialized) {
        g_pending_map = zmap_init_pending_map(pmap_hash_u64, pmap_cmp_u64);
        g_pmap_initialized = 1;
    }
}

static inline void pmap_add(int cx, int cz) {
    pmap_ensure_init();
    uint64_t k = pmap_key(cx, cz);
    zmap_put_pending_map(&g_pending_map, k, 1);
}

static inline void pmap_remove(int cx, int cz) {
    if(!g_pmap_initialized) return;
    uint64_t k = pmap_key(cx, cz);
    zmap_remove_pending_map(&g_pending_map, k);
}

static inline int pmap_check(int cx, int cz) {
    if(!g_pmap_initialized) return 0;
    uint64_t k = pmap_key(cx, cz);
    return zmap_get_pending_map(&g_pending_map, k) != NULL;
}

#endif
