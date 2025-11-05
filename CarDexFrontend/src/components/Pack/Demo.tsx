import React from "react";
import Pack from "./Pack";
import "../../App.css";

export default function PackShowcase() {
  const handlePackClick = (packName: string) => {
    alert(`Opening ${packName}!`);
  };

  return (
    <div className="bg-gradient-dark" style={{ minHeight: "100vh", padding: "48px" }}>
      <div style={{ maxWidth: "1200px", margin: "0 auto" }}>
        <h1 className="titlecase" style={{ marginBottom: "48px", textAlign: "center" }}>
          PACK SHOP
        </h1>

        {/* Pack Grid */}
        <div style={{ 
          display: "grid", 
          gridTemplateColumns: "repeat(auto-fill, minmax(200px, 1fr))",
          gap: "32px",
          justifyItems: "center"
        }}>
          
          {/* Example Pack 1 - Standard */}
          <Pack
            name="Starter Pack"
            packType="BOOSTER PACK"
            imageUrl="https://cdn.discordapp.com/attachments/1167885910168309871/1433596745719484426/image.png?ex=69054483&is=6903f303&hm=448876d9d8a3d44d67123e26bf44c9ccb8ea6d254f1b7e4b1f381b18bcc46463"
            price={2500}
            onClick={() => handlePackClick("Starter Pack")}
          />

          {/* Example Pack 2 - With Icon */}
          <Pack
            name="Premium Collection"
            packType="BOOSTER PACK"
            imageUrl="https://cdn.discordapp.com/attachments/1167885910168309871/1433602063475937422/image.png?ex=69054976&is=6903f7f6&hm=3457f7ace9651e3bbd13de98407e60c65f51574c7b90f1b346913a6d1c120c8c"
            price={5000}
            onClick={() => handlePackClick("Premium Collection")}
          />

          {/* Example Pack 3 - Expensive */}
          <Pack
            name="Ultra Rare Pack"
            packType="LIMITED EDITION"
            imageUrl="https://cdn.discordapp.com/attachments/1167885910168309871/1433602063475937422/image.png?ex=69054976&is=6903f7f6&hm=3457f7ace9651e3bbd13de98407e60c65f51574c7b90f1b346913a6d1c120c8c"
            price={15000}
            onClick={() => handlePackClick("Ultra Rare Pack")}
          />

          {/* Example Pack 4 */}
          <Pack
            name="Daily Special"
            packType="BOOSTER PACK"
            imageUrl="https://cdn.discordapp.com/attachments/1167885910168309871/1433602063475937422/image.png?ex=69054976&is=6903f7f6&hm=3457f7ace9651e3bbd13de98407e60c65f51574c7b90f1b346913a6d1c120c8c"
            price={1000}
            onClick={() => handlePackClick("Daily Special")}
          />

          {/* Example Pack 5 */}
          <Pack
            name="JDM Legends"
            packType="SPECIAL PACK"
            imageUrl="https://cdn.discordapp.com/attachments/1167885910168309871/1433602063475937422/image.png?ex=69054976&is=6903f7f6&hm=3457f7ace9651e3bbd13de98407e60c65f51574c7b90f1b346913a6d1c120c8c"
            price={7500}
            onClick={() => handlePackClick("JDM Legends")}
          />

          {/* Example Pack 6 */}
          <Pack
            name="Nismo Edition"
            packType="EXCLUSIVE"
            imageUrl="https://cdn.discordapp.com/attachments/1167885910168309871/1433602063475937422/image.png?ex=69054976&is=6903f7f6&hm=3457f7ace9651e3bbd13de98407e60c65f51574c7b90f1b346913a6d1c120c8c"
            price={12000}
            onClick={() => handlePackClick("Nismo Edition")}
          />
        </div>

        {/* Usage Example Section */}
        <section style={{ marginTop: "80px" }}>
          <h2 className="header-1" style={{ marginBottom: "24px" }}>
            Integration Example
          </h2>
          
          <div style={{ 
            background: "var(--block-secondary)", 
            borderRadius: "12px", 
            padding: "24px",
            maxWidth: "600px"
          }}>
            <p className="body-1" style={{ marginBottom: "16px" }}>
              Click any pack to see the interaction!
            </p>
            <pre className="body-2" style={{ 
              background: "var(--block-tertiary)", 
              padding: "16px", 
              borderRadius: "8px",
              overflow: "auto",
              color: "var(--content-primary)"
            }}>
{`<Pack
  name="Starter Pack"
  packType="BOOSTER PACK"
  imageUrl="/packs/starter.png"
  price={2500}
  onClick={handleOpen}
/>`}
            </pre>
          </div>
        </section>
      </div>
    </div>
  );
}