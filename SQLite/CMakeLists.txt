cmake_minimum_required(VERSION 3.24.2)
project(SQLite.Interop VERSION 1.0.116.0)

add_library(${PROJECT_NAME} SHARED SQLite.Interop/src/generic/interop.c)
target_link_libraries(${PROJECT_NAME} m pthread dl)

set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Wl,--no-undefined")

add_compile_definitions(SQLITE_THREADSAFE=1
                        SQLITE_USE_URI=1
                        SQLITE_ENABLE_COLUMN_METADATA=1
                        SQLITE_ENABLE_STAT4=1
                        SQLITE_ENABLE_FTS3=1
                        SQLITE_ENABLE_LOAD_EXTENSION=1
                        SQLITE_ENABLE_RTREE=1
                        SQLITE_SOUNDEX=1
                        SQLITE_ENABLE_MEMORY_MANAGEMENT=1
                        SQLITE_ENABLE_API_ARMOR=1
                        SQLITE_ENABLE_DBSTAT_VTAB=1
                        SQLITE_ENABLE_STMTVTAB=1
                        SQLITE_ENABLE_UPDATE_DELETE_LIMIT=1
                        INTEROP_TEST_EXTENSION=1
                        INTEROP_EXTENSION_FUNCTIONS=1
                        INTEROP_VIRTUAL_TABLE=1
                        INTEROP_FTS5_EXTENSION=1
                        INTEROP_PERCENTILE_EXTENSION=1
                        INTEROP_TOTYPE_EXTENSION=1
                        INTEROP_REGEXP_EXTENSION=1
                        INTEROP_JSON1_EXTENSION=1
                        INTEROP_SHA1_EXTENSION=1
                        INTEROP_SESSION_EXTENSION=1
                        $<$<CONFIG:DEBUG>:INTEROP_LOG=1>
                        $<$<CONFIG:DEBUG>:SQLITE_DEBUG=1>
                        $<$<CONFIG:DEBUG>:SQLITE_ENABLE_EXPENSIVE_ASSERT=1>
                        $<$<CONFIG:DEBUG>:SQLITE_MEMDEBUG=1>)


set_target_properties(${PROJECT_NAME} PROPERTIES VERSION ${PROJECT_VERSION})

install (TARGETS ${PROJECT_NAME}
         ARCHIVE DESTINATION .
         LIBRARY DESTINATION .
         RUNTIME DESTINATION .)
install(FILES $<TARGET_FILE:${PROJECT_NAME}> DESTINATION . RENAME SQLite.Interop.dll)
