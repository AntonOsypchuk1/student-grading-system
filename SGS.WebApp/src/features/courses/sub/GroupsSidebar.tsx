import {Sidebar, SidebarItem, SidebarList} from "../../../components/shared/StyledComponents.ts";
import {StudentsGroupDto} from "../../../types/GroupTypes.ts";

interface GroupsSidebarProps {
  groups: StudentsGroupDto[];
  selectedGroupId: string | null;
  onGroupSelect: (groupId: string) => void;
}

const GroupsSidebar = ({groups, selectedGroupId, onGroupSelect}: GroupsSidebarProps
) => {
  return (
    <Sidebar>
      <h3>Групи</h3>
      <SidebarList>
        {groups.map(group => (
          <SidebarItem
            key={group.id}
            isActive={selectedGroupId === group.id}
            onClick={() => onGroupSelect(group.id)}
          >
            {group.name}
          </SidebarItem>
        ))}
      </SidebarList>
    </Sidebar>
  );
};

export default GroupsSidebar;