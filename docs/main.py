from pprint import pprint

def on_pre_page_macros(env):
  # check if the page is a service page
  if env.page.file.url.startswith('services/'):
    # match the service data by file name
    service = next((x for x in env.variables.services if x["id"] == env.page.file.name), None)
    # skip if no service data found
    if not service:
      return
    # set the page title
    env.page.title = service["name"]
    env.page.meta["title"] = service["name"]

def define_env(env):
  @env.filter
  def format_scheme(scheme):
    return '```' + scheme + '://```'
