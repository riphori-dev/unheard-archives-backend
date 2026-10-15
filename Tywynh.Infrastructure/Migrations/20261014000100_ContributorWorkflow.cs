using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tywynh.Infrastructure.Persistence;
namespace Tywynh.Infrastructure.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("20261014000100_ContributorWorkflow")]
public partial class ContributorWorkflow : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.CreateTable("SubmissionReceipts", columns: t => new { Id = t.Column<Guid>(nullable:false), ConfessionId = t.Column<Guid>(nullable:false), IdempotencyHash = t.Column<string>(maxLength:64,nullable:false), RequestHash = t.Column<string>(maxLength:64,nullable:false), ReceiptHash = t.Column<string>(maxLength:64,nullable:false), ArchiveConsent = t.Column<bool>(nullable:false), TikTokConsent = t.Column<bool>(nullable:false), ConsentedAt = t.Column<DateTime>(nullable:false), ConsentVersion = t.Column<string>(maxLength:30,nullable:false) }, constraints: t => { t.PrimaryKey("PK_SubmissionReceipts", x => x.Id); t.ForeignKey("FK_SubmissionReceipts_confessions_ConfessionId", x => x.ConfessionId, "confessions", "id", onDelete: ReferentialAction.Cascade); });
        m.CreateIndex("IX_SubmissionReceipts_IdempotencyHash", "SubmissionReceipts", "IdempotencyHash", unique:true);
        m.CreateIndex("IX_SubmissionReceipts_ConfessionId", "SubmissionReceipts", "ConfessionId", unique:true);
        m.CreateTable("Publications", columns: t => new { Id=t.Column<Guid>(nullable:false), ConfessionId=t.Column<Guid>(nullable:false), Excerpt=t.Column<string>(maxLength:500,nullable:false), ReviewedAt=t.Column<DateTime>(nullable:false), PublishedAt=t.Column<DateTime>(nullable:true), PostUrl=t.Column<string>(maxLength:1000,nullable:true), RemovalRequestedAt=t.Column<DateTime>(nullable:true), RemovedAt=t.Column<DateTime>(nullable:true) }, constraints:t=>{t.PrimaryKey("PK_Publications",x=>x.Id);t.ForeignKey("FK_Publications_confessions_ConfessionId",x=>x.ConfessionId,"confessions","id",onDelete:ReferentialAction.Cascade);});
        m.CreateIndex("IX_Publications_ConfessionId","Publications","ConfessionId");
        m.CreateTable("ContentReports", columns:t=>new {Id=t.Column<Guid>(nullable:false),ConfessionId=t.Column<Guid>(nullable:false),Reason=t.Column<string>(maxLength:100,nullable:false),Details=t.Column<string>(maxLength:500,nullable:true),CreatedAt=t.Column<DateTime>(nullable:false),ResolvedAt=t.Column<DateTime>(nullable:true)},constraints:t=>{t.PrimaryKey("PK_ContentReports",x=>x.Id);t.ForeignKey("FK_ContentReports_confessions_ConfessionId",x=>x.ConfessionId,"confessions","id",onDelete:ReferentialAction.Cascade);});
        m.CreateIndex("IX_ContentReports_ConfessionId","ContentReports","ConfessionId");
    }
    protected override void Down(MigrationBuilder m) { m.DropTable("ContentReports"); m.DropTable("Publications"); m.DropTable("SubmissionReceipts"); }
}
