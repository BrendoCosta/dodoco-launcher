add_executable(Starter WIN32 "Starter.cpp")
set_target_properties(Starter PROPERTIES SUFFIX ".exe")
target_link_libraries(Starter -mwindows -lpsapi -lshlwapi)
target_link_libraries(Starter DodocoCommon)