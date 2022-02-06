﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ORM;

namespace ORM.Migrations
{
    [DbContext(typeof(MainContext))]
    partial class MainContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.13");

            modelBuilder.Entity("ORM.DbModels.Plan", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("BambooPlanName")
                        .HasColumnType("TEXT")
                        .HasColumnName("bamboo_plan_name");

                    b.Property<int>("BuildEndCount")
                        .HasColumnType("INTEGER")
                        .HasColumnName("build_end_count");

                    b.Property<int>("BuildStartCount")
                        .HasColumnType("INTEGER")
                        .HasColumnName("build_start_count");

                    b.Property<string>("PreviousCommits")
                        .HasColumnType("TEXT")
                        .HasColumnName("previous_commits");

                    b.Property<ulong?>("RelatedChatId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("related_chat_id");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("plan");
                });

            modelBuilder.Entity("ORM.DbModels.Stend", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("StendName")
                        .HasColumnType("TEXT")
                        .HasColumnName("stend_name");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT")
                        .HasColumnName("url");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("stend");
                });

            modelBuilder.Entity("ORM.DbModels.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasColumnName("login");

                    b.HasKey("Id");

                    b.ToTable("user");
                });

            modelBuilder.Entity("ORM.DbModels.UserInfo", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("BambooProjectName")
                        .HasColumnType("TEXT")
                        .HasColumnName("bamboo_project_name");

                    b.Property<string>("BambooToken")
                        .HasColumnType("TEXT")
                        .HasColumnName("bamboo_token");

                    b.Property<string>("JiraToken")
                        .HasColumnType("TEXT")
                        .HasColumnName("jira_token");

                    b.Property<ulong?>("MainChatId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("main_chat_id");

                    b.Property<string>("PluginName")
                        .HasColumnType("TEXT")
                        .HasColumnName("plugin_name");

                    b.Property<ulong?>("ServerId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("server_id");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("user_info");
                });

            modelBuilder.Entity("ORM.DbModels.Plan", b =>
                {
                    b.HasOne("ORM.DbModels.User", "User")
                        .WithMany("Plans")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ORM.DbModels.Stend", b =>
                {
                    b.HasOne("ORM.DbModels.User", "User")
                        .WithMany("Stends")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ORM.DbModels.UserInfo", b =>
                {
                    b.HasOne("ORM.DbModels.User", "User")
                        .WithOne("Info")
                        .HasForeignKey("ORM.DbModels.UserInfo", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ORM.DbModels.User", b =>
                {
                    b.Navigation("Info");

                    b.Navigation("Plans");

                    b.Navigation("Stends");
                });
#pragma warning restore 612, 618
        }
    }
}
