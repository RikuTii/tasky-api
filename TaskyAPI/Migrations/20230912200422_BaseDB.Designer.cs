﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskyAPI.Data;

#nullable disable

namespace TaskyAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230912200422_BaseDB")]
    partial class BaseDB
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("TaskyAPI.Models.File", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("File");
                });

            modelBuilder.Entity("TaskyAPI.Models.Notification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("ReceiverId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ReceiverId");

                    b.ToTable("Notification");
                });

            modelBuilder.Entity("TaskyAPI.Models.Task", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("CreatorId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<int?>("IsPast")
                        .HasColumnType("int");

                    b.Property<int>("Ordering")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ScheduleDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.Property<int>("TaskListId")
                        .HasColumnType("int");

                    b.Property<long?>("TimeElapsed")
                        .HasColumnType("bigint");

                    b.Property<long?>("TimeEstimate")
                        .HasColumnType("bigint");

                    b.Property<int?>("TimeTrack")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("TaskListId");

                    b.ToTable("Task");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("CreatorId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("TaskList");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskListMeta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int?>("TaskListId")
                        .HasColumnType("int");

                    b.Property<int?>("UserAccountId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TaskListId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("TaskListMeta");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskMeta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("FileId")
                        .HasColumnType("int");

                    b.Property<int>("TaskId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("FileId")
                        .IsUnique();

                    b.HasIndex("TaskId");

                    b.ToTable("TaskMeta");
                });

            modelBuilder.Entity("TaskyAPI.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("longtext");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("TaskyAPI.Models.UserAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Avatar")
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .HasColumnType("longtext");

                    b.Property<string>("LastName")
                        .HasColumnType("longtext");

                    b.Property<string>("Locale")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserAccount");
                });

            modelBuilder.Entity("TaskyAPI.Models.UserDevice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("AuthToken")
                        .HasColumnType("longtext");

                    b.Property<string>("FcmToken")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("LastActive")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("longtext");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("UserDevice");
                });

            modelBuilder.Entity("TaskyAPI.Models.Notification", b =>
                {
                    b.HasOne("TaskyAPI.Models.UserAccount", "Receiver")
                        .WithMany()
                        .HasForeignKey("ReceiverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Receiver");
                });

            modelBuilder.Entity("TaskyAPI.Models.Task", b =>
                {
                    b.HasOne("TaskyAPI.Models.UserAccount", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskyAPI.Models.TaskList", "TaskList")
                        .WithMany("Tasks")
                        .HasForeignKey("TaskListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");

                    b.Navigation("TaskList");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskList", b =>
                {
                    b.HasOne("TaskyAPI.Models.UserAccount", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskListMeta", b =>
                {
                    b.HasOne("TaskyAPI.Models.TaskList", "TaskList")
                        .WithMany("TaskListMetas")
                        .HasForeignKey("TaskListId");

                    b.HasOne("TaskyAPI.Models.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId");

                    b.Navigation("TaskList");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskMeta", b =>
                {
                    b.HasOne("TaskyAPI.Models.File", "File")
                        .WithOne()
                        .HasForeignKey("TaskyAPI.Models.TaskMeta", "FileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TaskyAPI.Models.Task", null)
                        .WithMany("Meta")
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("File");
                });

            modelBuilder.Entity("TaskyAPI.Models.UserAccount", b =>
                {
                    b.HasOne("TaskyAPI.Models.User", "User")
                        .WithMany("Accounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("TaskyAPI.Models.UserDevice", b =>
                {
                    b.HasOne("TaskyAPI.Models.UserAccount", "Account")
                        .WithMany("Devices")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("TaskyAPI.Models.Task", b =>
                {
                    b.Navigation("Meta");
                });

            modelBuilder.Entity("TaskyAPI.Models.TaskList", b =>
                {
                    b.Navigation("TaskListMetas");

                    b.Navigation("Tasks");
                });

            modelBuilder.Entity("TaskyAPI.Models.User", b =>
                {
                    b.Navigation("Accounts");
                });

            modelBuilder.Entity("TaskyAPI.Models.UserAccount", b =>
                {
                    b.Navigation("Devices");
                });
#pragma warning restore 612, 618
        }
    }
}