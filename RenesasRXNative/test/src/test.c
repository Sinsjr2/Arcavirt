#include "r_smc_entry.h"
#include "virtual_console_sandbox.h"

void v1Sandbox(void);


void main(void);

void main(void)
{
	v1Sandbox();
	VirtualConsoleSandbox_run();
}
