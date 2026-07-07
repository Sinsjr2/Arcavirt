cmake_minimum_required(VERSION 3.8)
set(CMAKE_C_STANDARD 99)
project(cmake_test LANGUAGES C ASM)

file(GLOB_RECURSE SOURCES
    "src/*.c"
    "src/*.S")

set(RX_MCPU "rx64m" CACHE STRING "RX MCU variant passed to -mcpu (e.g. rx64m)")
SET(CMAKE_ASM_FLAGS "-D__RXV2 -mcpu=${RX_MCPU} -nostartfiles -mlittle-endian-data")
SET(CMAKE_C_FLAGS "${CMAKE_ASM_FLAGS} -ffunction-sections -fdata-sections -fdiagnostics-parseable-fixits")
SET(CMAKE_CXX_FLAGS ${CMAKE_C_FLAGS})

add_executable(cmake_test ${SOURCES})
# target_link_options(cmake_test PRIVATE
#     -T ${CMAKE_SOURCE_DIR}/../src/linker_script.ld
# )

target_include_directories(cmake_test PRIVATE
    src/smc_gen/
    src/smc_gen/general/
    src/smc_gen/r_config/
    src/smc_gen/r_pincfg/
    src/smc_gen/r_bsp/
    src/smc_gen/r_bsp/board/generic_rx64m/
    src/smc_gen/r_bsp/mcu/all/
    src/smc_gen/r_bsp/mcu/rx64m/
    src/
    src/asm_sandbox/)

target_link_options(cmake_test PRIVATE -T ${CMAKE_SOURCE_DIR}/src/linker_script.ld)
# set_target_properties(cmake_test PROPERTIES LINK_DEPENDS 
#     ${CMAKE_SOURCE_DIR}/src/linker_script.ld)