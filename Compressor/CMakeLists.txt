cmake_minimum_required(VERSION 3.24.2)
project(compressor VERSION 1.0.0)

add_library(${PROJECT_NAME} SHARED src/compressor.cpp)
target_link_libraries(${PROJECT_NAME} z lzma bz2)

set_property(TARGET ${PROJECT_NAME} PROPERTY CXX_STANDARD 20)

set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Wl,--no-undefined")

set_target_properties(${PROJECT_NAME} PROPERTIES VERSION ${PROJECT_VERSION})

install (TARGETS ${PROJECT_NAME}
         ARCHIVE DESTINATION .
         LIBRARY DESTINATION .
         RUNTIME DESTINATION .)
